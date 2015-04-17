using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Orion.Zeta.Core.SearchMethods.Shared;

namespace Orion.Zeta.Core.SearchMethods.ApplicationSearch {
	public class ApplicationsContainer {
		private readonly string _path;
		private readonly IEnumerable<string> _patterns;
		private readonly IFileSystemSearch _fileSystemSearch;

		public ApplicationsContainer(string path, IEnumerable<string> patterns, IFileSystemSearch fileSystemSearch) {
			if (!fileSystemSearch.DirectoryExists(path)) {
				throw new DirectoryNotFoundException(path);
			}
			if (fileSystemSearch == null) {
				throw new ArgumentNullException();
			}
			this._path = path;
			this._patterns = patterns;
			this._fileSystemSearch = fileSystemSearch;
		}

		public IEnumerable<IItem> Search(string expression) {
			var regex = new Regex(this.ConvertWildcardToRegex(this.ConvertExpressionToWildCard(expression)), RegexOptions.IgnoreCase);
			var matchedFiles = this.GetMatchedFiles();
			var items = matchedFiles.Select(mf => {
				var item = new Item {
					Value = mf,
					DisplayName = this._fileSystemSearch.GetFilename(mf),
					Icon = this._fileSystemSearch.GetIcon(mf),
					Execute = new Execute {
						Program = mf
					}
				};
				return item;
			});
			return items.Where(mf => regex.IsMatch(mf.DisplayName));;
		}

		private string ConvertExpressionToWildCard(string expression) {
			var chars = new char[expression.Length * 2 + 1];
			chars[0] = '*';
			var position = 1;
			foreach (var @char in expression) {
				chars[position] = @char;
				++position;
				chars[position] = '*';
				++position;
			}
			return new string(chars);
		}

		private IEnumerable<string> GetMatchedFiles() {
			var files = this._fileSystemSearch.GetFiles(this._path, "*", SearchOption.AllDirectories);

			var regexs = this._patterns.Select(pattern => new Regex(this.ConvertWildcardToRegex(pattern))).ToList();
			return files.Where(f => regexs.Any(r => r.IsMatch(f)));
		}

		private string ConvertWildcardToRegex(string wildcard) {
			return "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
		}

		public string Path {
			get { return this._path; }
		}
	}
}