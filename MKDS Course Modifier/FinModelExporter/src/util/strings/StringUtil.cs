﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fin.util.strings {
  public static class StringUtil {

    public static string Repeat(string str, int times) {
      if (times == 0) {
        return "";
      }

      if (times == 1) {
        return str;
      }

      var builder = new StringBuilder();
      for (var i = 0; i < times; ++i) {
        builder.Append(str);
      }
      return builder.ToString();
    }

    public static string Concat(IEnumerable<string> strs, string separator = "") {
      if (!strs.Any()) {
        return "";
      }

      var builder = new StringBuilder();

      builder.Append(strs.First());

      foreach (var str in strs.Skip(1)) {
        builder.Append(separator);
        builder.Append(str);
      }

      return builder.ToString();
    }
  }
}