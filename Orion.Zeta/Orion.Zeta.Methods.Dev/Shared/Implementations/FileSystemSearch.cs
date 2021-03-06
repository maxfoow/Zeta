using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Orion.Zeta.Methods.Dev.Shared.Helpers;

namespace Orion.Zeta.Methods.Dev.Shared.Implementations {
	public class FileSystemSearch : IFileSystemSearch {
		public IEnumerable<string> GetFiles(string path, string pattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) {
			return Directory.GetFiles(path, pattern, searchOption);
		}

		public IEnumerable<string> GetDirectories(string path, string pattern, SearchOption searchOption = SearchOption.TopDirectoryOnly) {
			return Directory.GetDirectories(path, pattern, searchOption);
		}

		public bool DirectoryExists(string path) {
			return Directory.Exists(path);
		}

		public string GetFilename(string path) {
			return Path.GetFileNameWithoutExtension(path);
		}

		public Icon GetIcon(string path) {
			return IconHelper.GetIcon(path);
		}
	}
}