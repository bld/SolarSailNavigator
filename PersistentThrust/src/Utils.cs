using System;
using System.Linq;
using UnityEngine;

namespace PersistentThrust {

    public class Utils {

	// Format thrust into mN, N, kN
	public static string FormatThrust(double thrust) {
	    if (thrust < 0.001) {
		return Math.Round(thrust * 1000000.0, 3).ToString() + " mN";
	    } else if (thrust < 1.0) {
		return Math.Round(thrust * 1000.0, 3).ToString() + " N";
	    } else {
		return Math.Round(thrust, 3).ToString() + " kN";
	    }
	}
    }
}
