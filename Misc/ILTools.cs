using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.Misc
{
    public static class ILTools
    {
        public static bool JumpToNext(this ILCursor crs, Func<Instruction, bool> match, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                if (!crs.TryGotoNext(MoveType.After, match))
                    return false;
            }

            return true;
        }

        public static bool JumpBeforeNext(this ILCursor crs, Func<Instruction, bool> match, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                if (!crs.TryGotoNext(i == times - 1 ? MoveType.Before : MoveType.After, match))
                    return false;
            }

            return true;
        }

        public static IEnumerable MatchAfter(this ILCursor crs, Func<Instruction, bool> match)
        {
            var curr = crs.Next;
            crs.Index = 0;

            while (crs.JumpToNext(match))
                yield return null;

            crs.Next = curr;
        }

        public static IEnumerable MatchBefore(this ILCursor crs, Func<Instruction, bool> match)
        {
            var curr = crs.Next;
            crs.Index = 0;

            while (crs.JumpBeforeNext(match))
            {
                var c = crs.Next;

                yield return null;
                crs.Goto(c, MoveType.After);
            }

            crs.Next = curr;
        }

        public static VariableDefinition DeclareLocal<T>(this ILContext ctx)
        {
            var loc = new VariableDefinition(ctx.Import(typeof(T)));
            ctx.Body.Variables.Add(loc);

            return loc;
        }

        public static VariableDefinition DeclareLocal<T>(this ILCursor crs)
        {
            return ILTools.DeclareLocal<T>(crs);
        }

        public static bool TryGotoArg(this ILCursor crs, Instruction targetInstr, int argIndex, int instance = 0)
        {
            if (argIndex < 0)
                return false;

            if (instance < 0)
                return false;

            if (targetInstr == null)
                return false;

            var argumentInstrs = targetInstr.GetArgumentInstructions(crs.Context, argIndex);

            if (instance >= argumentInstrs.Count)
                return false;

            crs.Goto(argumentInstrs[instance], MoveType.After);
            return true;
        }

        public static bool TryGotoArg(this ILCursor crs, int argIndex, int instance = 0)
        {
            return crs.TryGotoArg(crs.Next, argIndex, instance);
        }

        public static IEnumerable MatchArg(this ILCursor crs, Instruction targetInstr, int argIndex)
        {
            if (argIndex < 0)
                yield break;

            if (targetInstr == null)
                yield break;

            var curr = crs.Next;
            var argumentInstrs = targetInstr.GetArgumentInstructions(crs.Context, argIndex);

            foreach (var arg in argumentInstrs)
            {
                crs.Goto(arg, MoveType.After);

                yield return null;
            }

            crs.Next = curr;
        }

        public static IEnumerable MatchArg(this ILCursor crs, int argIndex)
        {
            return crs.MatchArg(crs.Next, argIndex);
        }

        private static List<Instruction> GetArgumentInstructions(this Instruction instruction, ILContext context, int argumentIndex)
        {
            var args = instruction.InputCount();
            var moves = args - argumentIndex - 1;

            if (moves < 0)
            {
                Debug.Log($"Argument index {argumentIndex} is higher than the highest argument index ({args - 1})");

                return [];
            }

            var prev = instruction.PossiblePreviousInstructions(context);
            var argInstrs = new List<Instruction>();

            foreach (var i in prev)
                BacktrackToArg(i, context, moves, argInstrs);

            argInstrs.Sort((a, b) => context.IndexOf(a).CompareTo(context.IndexOf(b)));

            return argInstrs;
        }

        private static void BacktrackToArg(Instruction current, ILContext ctx, int remainingMoves, List<Instruction> foundArgs)
        {
            if (remainingMoves <= 0 && current.OutputCount() > 0)
            {
                if (remainingMoves == 0)
                    foundArgs.Add(current);

                return;
            }

            remainingMoves -= current.StackDelta();
            var prev = current.PossiblePreviousInstructions(ctx);

            foreach (var i in prev)
                BacktrackToArg(i, ctx, remainingMoves, foundArgs);
        }

        public static int InputCount(this Instruction instr)
        {
            if (instr == null)
                return 0;

            var op = instr.OpCode;

            if (op.FlowControl == FlowControl.Call)
            {
                var mthd = (IMethodSignature)instr.Operand;
                var ins = 0;

                if (op.Code != Code.Newobj && mthd.HasThis && !mthd.ExplicitThis)
                    ins++; // Input the "self" arg

                if (mthd.HasParameters)
                    ins += mthd.Parameters.Count; // Input all of the parameters

                if (op.Code == Code.Calli)
                    ins++; // No clue for this one

                return ins;
            }

            return op.StackBehaviourPop switch
            {
                StackBehaviour.Pop1 or StackBehaviour.Popi or StackBehaviour.Popref => 1,
                StackBehaviour.Pop1_pop1 or StackBehaviour.Popi_pop1 or StackBehaviour.Popi_popi or StackBehaviour.Popi_popi8 or StackBehaviour.Popi_popr4 or StackBehaviour.Popi_popr8 or StackBehaviour.Popref_pop1 or StackBehaviour.Popref_popi => 2,
                StackBehaviour.Popi_popi_popi or StackBehaviour.Popref_popi_popi or StackBehaviour.Popref_popi_popi8 or StackBehaviour.Popref_popi_popr4 or StackBehaviour.Popref_popi_popr8 or StackBehaviour.Popref_popi_popref => 3,

                _ => 0,
            };
        }

        public static int OutputCount(this Instruction instr)
        {
            if (instr == null)
                return 0;

            var op = instr.OpCode;

            if (op.FlowControl == FlowControl.Call)
            {
                var mthd = (IMethodSignature)instr.Operand;
                var outs = 0;

                if (op.Code == Code.Newobj || mthd.ReturnType.MetadataType != MetadataType.Void)
                    outs++; // Output the return value

                return outs;
            }

            return op.StackBehaviourPush switch
            {
                StackBehaviour.Push1 or StackBehaviour.Pushi or StackBehaviour.Pushi8 or StackBehaviour.Pushr4 or StackBehaviour.Pushr8 or StackBehaviour.Pushref => 1,
                StackBehaviour.Push1_push1 => 2,

                _ => 0,
            };
        }

        public static int StackDelta(this Instruction instr)
        {
            return instr.OutputCount() - instr.InputCount();
        }

        public static List<Instruction> PossiblePreviousInstructions(this Instruction instr, ILContext ctx)
        {
            var l = new List<Instruction>();

            foreach (var i in ctx.Instrs)
            {
                if (Array.IndexOf(i.PossibleNextInstructions(), instr) >= 0)
                    l.Add(i);
            }

            return l;
        }

        public static Instruction[] PossibleNextInstructions(this Instruction instr)
        {
            return instr.OpCode.FlowControl switch
            {
                FlowControl.Next or FlowControl.Call => [instr.Next],
                FlowControl.Branch => instr.GetBranchTarget() is Instruction tr ? [tr] : [],
                FlowControl.Cond_Branch => instr.GetBranchTarget() is Instruction tr ? [instr.Next, tr] : [instr.Next],

                _ => []
            };
        }

        public static Instruction GetBranchTarget(this Instruction branch)
        {
            if (branch.Operand is Instruction tr)
                return tr;

            if (branch.Operand is ILLabel lb)
                return lb.Target;

            return null;
        }

        public static string InstructionToString(this Instruction c)
        {
            try
            {
                return c.ToString();
            }
            catch
            {
                try
                {
                    if (c.OpCode.OperandType is OperandType.InlineBrTarget or OperandType.ShortInlineBrTarget && c.Operand is ILLabel l)
                        return $"IL_{c.Offset:x4}: {c.OpCode.Name} IL_{l.Target.Offset:x4}";

                    if (c.OpCode.OperandType is OperandType.InlineSwitch && c.Operand is IEnumerable<ILLabel> en)
                        return $"IL_{c.Offset:x4}: {c.OpCode.Name} {string.Join(", ", en.Select(x => x.Target.Offset.ToString("x4")).ToArray())}";
                }
                catch { }
            }

            return "This shouldn't be happening";
        }

        public static T EnumeratorGetField<T>(this object obj, string name) => (T)obj.GetType().EnumeratorField(name).GetValue(obj);
        public static FieldInfo EnumeratorField(this MethodBase method, string name) => method.DeclaringType.EnumeratorField(name);
        public static FieldInfo EnumeratorField(this Type tp, string name) => tp.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First(x => x != null && x.Name != null && (x.Name.Contains($"<{name}>") || x.Name == name));
    }
}