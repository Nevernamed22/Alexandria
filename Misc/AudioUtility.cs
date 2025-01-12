using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Alexandria.Misc
{
    public static class AudioUtility
    {
		public static void AutoloadFromAssembly(Assembly assembly, string prefix)
		{
			if (assembly == null) { throw new ArgumentNullException("assembly", "Assembly cannot be null."); }
			if (prefix == null) { throw new ArgumentNullException("prefix", "Prefix name cannot be null."); }

			prefix = prefix.Trim();
			if (prefix == "") { throw new ArgumentException("Prefix name cannot be an empty (or whitespace only) string.", "prefix"); }

			char sep = Path.DirectorySeparatorChar;
			foreach (string resName in assembly.GetManifestResourceNames())
			{
				if (!resName.StartsWith(prefix))
					continue;

				string bankName = resName.Substring(prefix.Length).Replace('/', sep).Replace('\\', sep);
				if (bankName.LastIndexOf(".bnk") != bankName.Length - ".bnk".Length)
					continue;

				bankName = bankName.Substring(0, bankName.Length - ".bnk".Length);
				if (bankName[0] == sep)
					bankName = bankName.Substring(1);
				bankName = prefix + ":" + bankName;
				// Console.WriteLine(string.Format("{0}: Soundbank found, attempting to autoload: name='{1}' resource='{2}'", typeof(ResourceLoaderSoundbanks), text2, text));
				using (Stream manifestResourceStream = assembly.GetManifestResourceStream(resName))
				{
					LoadSoundbankFromStream(manifestResourceStream, bankName);
				}
			}
		}

		private static unsafe void LoadSoundbankFromStream(Stream stream, string name)
		{
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            fixed (byte* p = array)
                AkSoundEngine.LoadAndDecodeBankFromMemory((IntPtr)p, (uint)array.Length, false, name, false, out _);
		}

        [Obsolete("StreamToByteArray() is obsolete and exists for backwards compatability only.", false)]
		public static byte[] StreamToByteArray(Stream input)
		{
			byte[] array = new byte[16384];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int count;
				while ((count = input.Read(array, 0, array.Length)) > 0) { memoryStream.Write(array, 0, count); }
				result = memoryStream.ToArray();
			}
			return result;
		}
	}
}
