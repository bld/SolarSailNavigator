using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    /*
    public class SailControls {

	// Fields

	public int ncontrols;
	public SailControl[] controls;

	// Constructor

	public SailControls (int n, ModuleSolarSail sail) {
	    ncontrols = n;
	    controls = new SailControl[n];
	    for(int i = 0; i < n; i++) {
		controls[i] = new SailControl(sail);
	    }
	}

	// GUI

	public void GUI() {
	    GUILayout.BeginVertical();
	    GUILayout.Label("Cone Clock Duration Warp");
	    foreach(var control in controls) {
		control.GUILine();
	    }
	    GUILayout.EndVertical();
	}

	private char delimiter = ':';
	
	// Turns controls to strings and return as array
	public string[] ExportControls () {
	    var cones = controls[0].coneAngle_str;
	    var clocks = controls[0].clockAngle_str;
	    var durations = controls[0].duration_str;
	    var factors = controls[0].factor_str;
	    for (var i = 1; i < ncontrols; i++) {
		cones += delimiter + controls[i].coneAngle_str;
		clocks += delimiter + controls[i].clockAngle_str;
		durations += delimiter + controls[i].duration_str;
		factors += delimiter + controls[i].factor_str;
	    }
	    return new string[] { cones, clocks, durations, factors };
	}

	// Import controls strings and populate fields
	public void ImportControls (string cones, string clocks, string durations, string factors) {
	    var coneStrings = cones.Split(delimiter);
	    var clockStrings = clocks.Split(delimiter);
	    var durationStrings = durations.Split(delimiter);
	    var factorStrings = factors.Split(delimiter);
	    ncontrols = Math.Min(coneStrings.Length, Math.Min(clockStrings.Length, Math.Min(durationStrings.Length, factorStrings.Length)));
	    for(var i = 0; i < ncontrols; i++) {
		
	    }
	}
    }
    */
    
    public class SailControl {

	// Fields
	// Cone Angle
	public float coneAngle;
	public string coneAngle_str;
	// Clock angle
	public float clockAngle;
	public string clockAngle_str;
	// Duration of steering maneuver
	public double duration;
	public string duration_str;
	// Warp factor
	public double factor;
	public string factor_str;
	public ModuleSolarSail sail; // Sail this control is attached to

	// Static fields
	public static string defaultCone = "90";
	public static string defaultClock = "0";
	public static string defaultDuration = "100000";
	public static string defaultFactor = "100000";

	// Tilt controls
	
	public void TiltPlus() {
	    coneAngle += 5;
	    if (coneAngle > 90) {
		coneAngle = coneAngle - 180;
	    }
	}

	public void TiltMinus() {
	    coneAngle -= 5;
	    if (coneAngle < -90) {
		coneAngle = coneAngle + 180;
	    }
	}

	// Rotate controls

	public void RotatePlus() {
	    clockAngle += 5;
	    if (clockAngle > 180) {
		clockAngle = clockAngle - 360;
	    }
	}

	public void RotateMinus() {
	    clockAngle -= 5;
	    if (clockAngle < -180) {
		clockAngle = 360 + clockAngle;
	    }
	}

	// GUI line
	public void GUILine () {
	    // Line
	    GUILayout.BeginHorizontal();
	    // Cone
	    GUILayout.Label(coneAngle_str);
	    if (GUILayout.Button("+")) {
		TiltPlus();
		coneAngle_str = coneAngle.ToString();
		sail.cone = coneAngle_str; // Update sail's cone string
	    }
	    if (GUILayout.Button("-")) {
		TiltMinus();
		coneAngle_str = coneAngle.ToString();
		sail.cone = coneAngle_str; // Update sail's cone string
	    }
	    // Clock
	    GUILayout.Label(clockAngle_str);
	    if (GUILayout.Button("+")) {
		RotatePlus();
		clockAngle_str = clockAngle.ToString();
		sail.clock = clockAngle_str; // Update sail's clock string
	    }
	    if (GUILayout.Button("-")) {
		RotateMinus();
		clockAngle_str = clockAngle.ToString();
		sail.clock = clockAngle_str; // Update sail's clock string
	    }
	    // Duration
	    duration_str = GUILayout.TextField(duration_str, 25);
	    double tmpd;
	    if (Double.TryParse(duration_str, out tmpd)) {
		duration = tmpd;
		sail.duration = duration_str; // Update sail's duration string
	    }
	    // Warp factor
	    factor_str = GUILayout.TextField(factor_str, 25);
	    double tmpf;
	    if (Double.TryParse(factor_str, out tmpf)) {
		factor = tmpf;
		sail.factor = factor_str; // Update sail's factor string
	    }
	    // End line
	    GUILayout.EndHorizontal();
	}

	// Parse a string to a single
	private static float ParseSingle (string str) {
	    float tmp;
	    if (Single.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0.0f;
	    }
	}

	// Parse a string to a double
	private static double ParseDouble (string str) {
	    double tmp;
	    if (Double.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0.0;
	    }
	}
	
	// Constructor
	public SailControl (ModuleSolarSail sailin) {
	    // Specify sail
	    sail = sailin;
	    // Read in persistant control data
	    // cone
	    coneAngle_str = sail.cone;
	    coneAngle = ParseSingle(coneAngle_str);
	    // clock
	    clockAngle_str = sail.clock;
	    clockAngle = ParseSingle(clockAngle_str);
	    // duration
	    duration_str = sail.duration;
	    duration = ParseDouble(duration_str);
	    // warp factor
	    factor_str = sail.factor;
	    factor = ParseDouble(factor_str);
	}
    }
}
