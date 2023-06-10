using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alexandria.Misc;

namespace Alexandria.EnemyAPI
{
    public static class AttackBehaviourUtility
    {
        public static bool IsAttackBehaviorGroup(this AttackBehaviorBase self, out List<AttackBehaviorBase> groupItems)
        {
            groupItems = new List<AttackBehaviorBase>();
            if (self is AttackBehaviorGroup)
            {
                foreach (AttackBehaviorBase behav2 in (self as AttackBehaviorGroup).FindAttackBehaviorsInGroup<AttackBehaviorBase>())
                {
                    groupItems.Add(behav2);
                }
                return true;
            }
            else if (self is SequentialAttackBehaviorGroup)
            {
                foreach (AttackBehaviorBase behav2 in (self as SequentialAttackBehaviorGroup).FindAttackBehaviorsInSequentialGroup<AttackBehaviorBase>())
                {
                    groupItems.Add(behav2);
                }
                return true;
            }
            else if (self is SimultaneousAttackBehaviorGroup)
            {
                foreach (AttackBehaviorBase behav2 in (self as SimultaneousAttackBehaviorGroup).FindAttackBehaviorsInSimultaneousGroup<AttackBehaviorBase>())
                {
                    groupItems.Add(behav2);
                }
                return true;
            }
            return false;
        }

        public static List<T> FindAttackBehaviorsInGroup<T>(this AttackBehaviorGroup group) where T : AttackBehaviorBase
        {
            if (group == null || group.AttackBehaviors == null)
            {
                return new List<T>();
            }
            List<T> result = new List<T>();
            foreach (AttackBehaviorGroup.AttackGroupItem item in group.AttackBehaviors)
            {
                if (item != null && item.Behavior != null)
                {
                    List<AttackBehaviorBase> groupItems;
                    if (item.Behavior is T)
                    {
                        result.Add(item.Behavior as T);
                    }
                    else if (item.Behavior.IsAttackBehaviorGroup(out groupItems))
                    {
                        foreach (AttackBehaviorBase behav in groupItems)
                        {
                            if (behav is T)
                            {
                                result.Add(behav as T);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static List<T> FindAttackBehaviorsInSequentialGroup<T>(this SequentialAttackBehaviorGroup group) where T : AttackBehaviorBase
        {
            if (group == null || group.AttackBehaviors == null)
            {
                return new List<T>();
            }
            List<T> result = new List<T>();
            foreach (AttackBehaviorBase item in group.AttackBehaviors)
            {
                List<AttackBehaviorBase> groupItems;
                if (item is T)
                {
                    result.Add(item as T);
                }
                else if (item.IsAttackBehaviorGroup(out groupItems))
                {
                    foreach (AttackBehaviorBase behav in groupItems)
                    {
                        if (behav is T)
                        {
                            result.Add(behav as T);
                        }
                    }
                }
            }
            return result;
        }

        public static List<T> FindAttackBehaviorsInSimultaneousGroup<T>(this SimultaneousAttackBehaviorGroup group) where T : AttackBehaviorBase
        {
            if (group == null || group.AttackBehaviors == null)
            {
                return new List<T>();
            }
            List<T> result = new List<T>();
            foreach (AttackBehaviorBase item in group.AttackBehaviors)
            {
                List<AttackBehaviorBase> groupItems;
                if (item is T)
                {
                    result.Add(item as T);
                }
                else if (item.IsAttackBehaviorGroup(out groupItems))
                {
                    foreach (AttackBehaviorBase behav in groupItems)
                    {
                        if (behav is T)
                        {
                            result.Add(behav as T);
                        }
                    }
                }
            }
            return result;
        }

        public static List<T> FindAttackBehaviors<T>(this BehaviorSpeculator spec) where T : AttackBehaviorBase
        {
            if (spec == null || spec.AttackBehaviors == null)
            {
                return new List<T>();
            }
            List<T> result = new List<T>();
            foreach (AttackBehaviorBase behav in spec.AttackBehaviors)
            {
                if (behav != null)
                {
                    List<AttackBehaviorBase> groupItems;
                    if (behav is T)
                    {
                        result.Add(behav as T);
                    }
                    else if (behav.IsAttackBehaviorGroup(out groupItems))
                    {
                        foreach (AttackBehaviorBase behav2 in groupItems)
                        {
                            if (behav2 is T)
                            {
                                result.Add(behav2 as T);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static AIActor GetAttackBehaviourOwner(this BehaviorBase behav)
        {
            return behav.m_aiActor;
        }
    }
}
