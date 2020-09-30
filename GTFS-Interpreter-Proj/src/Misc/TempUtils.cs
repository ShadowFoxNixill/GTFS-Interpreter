using System;
using System.Collections.Generic;

namespace Nixill.Utils {
  internal static class TempUtils {
    internal static List<R> Transform<T, R>(this List<T> input, Func<T, R> function) {
      List<R> ret = new List<R>();

      foreach (T t in input) {
        ret.Add(function(t));
      }

      return ret;
    }

    internal static double LerpDouble(double x0, double y0, double x1, double y1, double xT) => (xT - x0) * (y1 - y0) / (x1 - x0) + y0;
    internal static float LerpFloat(float x0, float y0, float x1, float y1, float xT) => (xT - x0) * (y1 - y0) / (x1 - x0) + y0;
    internal static decimal LerpDecimal(decimal x0, decimal y0, decimal x1, decimal y1, decimal xT) => (xT - x0) * (y1 - y0) / (x1 - x0) + y0;
  }
}