using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class SailControl {

	// Fields
	// Cone Angle
	public float cone;
	public string cone_str;
	// Clock angle
	public float clock;
	public string clock_str;
	// Duration of steering maneuver
	public double duration;
	public string duration_str;
	// Days
	double days;
	string days_str;
	// Warp factor
	public int iwarp; // Index of warpLevels array
	public double warp; // Warp factor
	// Color
	public Color color;
	// Controls object this control is attached to
	public SailControls controls;
	// Sail this controls
	public SolarSailPart sail;

	// Static fields
	public static float defaultCone = 90f;
	public static float defaultClock = 0f;
	public static double defaultDuration = 100000.0;
	public static double SecondsPerDay = 21600.0;
	public static double SecondsPerHour = 3600.0;
	public static double SecondsPerMinute = 60.0;
	public static double[] warpLevels = { 1, 2, 3, 4, 5, 10, 50, 100, 1000, 10000, 100000 };
	public static int defaultiwarp = 10;
	
	// Cone controls
	public void GUICone () {
	    GUILayout.Label(cone_str);
	    if (GUILayout.Button("+")) {
		cone += 5;
		if (cone > 90) {
		    cone = cone - 180;
		}
		cone_str = cone.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		cone -= 5;
		if (cone < -90) {
		    cone = cone + 180;
		}
		cone_str = cone.ToString();
		controls.Update();
	    }
	}

	// Clock controls
	public void GUIClock () {
	    GUILayout.Label(clock_str);
	    if (GUILayout.Button("+")) {
		clock += 5;
		if (clock > 180) {
		    clock = clock - 360;
		}
		clock_str = clock.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		clock -= 5;
		if (clock < -180) {
		    clock = 360 + clock;
		}
		clock_str = clock.ToString();
		controls.Update();
	    }
	}

	// Time controls
	public void GUITime () {

	    // Days
	    GUILayout.Label(days_str);

	    // Increase
	    GUILayout.BeginVertical();
	    if (GUILayout.Button("+")) {
		days++;
		days_str = days.ToString();
		duration = SecondsPerDay * days;
		duration_str = duration.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("+10")) {
		days += 10;
		days_str = days.ToString();
		duration = SecondsPerDay * days;
		duration_str = duration.ToString();
		controls.Update();
	    }
	    GUILayout.EndVertical();

	    // Decrease
	    GUILayout.BeginVertical();
	    if (GUILayout.Button("-")) {
		if (days > 0) {
		    days--;
		    days_str = days.ToString();
		    duration = SecondsPerDay * days;
		    duration_str = duration.ToString();
		    controls.Update();
		}
	    }
	    if (GUILayout.Button("-10")) {
		if (days >= 10) {
		    days -= 10;
		    days_str = days.ToString();
		    duration = SecondsPerDay * days;
		    duration_str = duration.ToString();
		    controls.Update();
		}
	    }
	    GUILayout.EndVertical();
	}

	// Warp factor controls
	public void GUIWarp () {
	    GUILayout.Label(warp.ToString());
	    if (GUILayout.Button("+")) {
		iwarp++;
		if (iwarp >= warpLevels.Length) {
		    iwarp = 0;
		}
		warp = warpLevels[iwarp];
		controls.Update();
	    }
	}

	// Color controls
	public void GUIColor (Color color) {
	    this.color = color;
	    var cstyle = new GUIStyle();
	    var ctx = new Texture2D(1, 1);
	    cstyle.normal.background = ctx;
	    ctx.SetPixel(1,1,color);
	    ctx.Apply();
	    GUILayout.Label("___", cstyle);
	}
	
	// GUI line
	public void GUILine (Color color) {
	    GUILayout.BeginHorizontal();

	    GUICone();
	    GUIClock();
	    GUITime();
	    GUIWarp();
	    GUIColor(color);

	    GUILayout.EndHorizontal();
	}
	
	// Parse a string to a single
	public static float ParseSingle (string str) {
	    float tmp;
	    if (Single.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0.0f;
	    }
	}

	// Parse a string to a double
	public static double ParseDouble (string str) {
	    double tmp;
	    if (Double.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0.0;
	    }
	}

	// Parse a string to an int
	public static int ParseInt (string str) {
	    int tmp;
	    if (Int32.TryParse(str, out tmp)) {
		return tmp;
	    } else {
		return 0;
	    }
	}

	// Constructor

	public SailControl(SolarSailPart sail, SailControls controls, float cone, float clock, double duration, int iwarp) {
	    // Sail
	    this.sail = sail;
	    this.cone = cone;
	    // Parent controls object
	    this.controls = controls;
	    // Angles
	    cone_str = cone.ToString();
	    this.clock = clock;
	    clock_str = clock.ToString();
	    // Time
	    this.duration = duration;
	    this.days = Math.Floor(duration / SecondsPerDay);
	    this.days_str = days.ToString();
	    this.duration = days * SecondsPerDay;
	    duration_str = duration.ToString();
	    // Warp factor
	    this.iwarp = iwarp;
	    warp = warpLevels[iwarp];
	}

	public static SailControl Default (SolarSailPart sail, SailControls controls) {
	    return new SailControl (sail, controls, defaultCone, defaultClock, defaultDuration, defaultiwarp);
	}
    }

    public class SailControls {

	// Fields

	public int ncontrols;
	public SailControl[] controls;
	public SolarSailPart sail;
	public double UT0;
	public SailControl sailOff;
	double durationTotal;
	public Color colorFinal;
	
	// Static fields
	static Color[] colorMap = { Color.yellow,
				    Color.red,
				    Color.green,
				    Color.magenta,
				    Color.grey,
				    Color.blue,
				    Color.white };

	// Delimiter between controls
	private char delimiter = ':';

	// Constructor

	// Give the sail to which this control is for
	public SailControls (SolarSailPart sail) {

	    // Assign sail field
	    this.sail = sail;
	    Debug.Log(this.sail.ToString());

	    // Initial time
	    if (sail.UT0 == 0) {
		UT0 = Planetarium.GetUniversalTime();
	    } else {
		UT0 = sail.UT0;
	    }
	    Debug.Log(UT0.ToString());

	    // Off sail control
	    sailOff = SailControl.Default(sail, this);

	    // If the sail doesn't have saved controls, return default
	    if (String.IsNullOrEmpty(sail.cones) ||
		String.IsNullOrEmpty(sail.clocks) ||
		String.IsNullOrEmpty(sail.durations) ||
		String.IsNullOrEmpty(sail.iwarps)) {
		ncontrols = 1;
		controls = new SailControl[ncontrols];
		controls[0] = SailControl.Default(sail, this);

	    } else { // Otherwise, parse saved controls

		// Split into arrays
		var coneStrings = sail.cones.Split(delimiter);
		var clockStrings = sail.clocks.Split(delimiter);
		var durationStrings = sail.durations.Split(delimiter);
		var iwarpStrings = sail.iwarps.Split(delimiter);

		// Find number of controls
		ncontrols = Math.Min(coneStrings.Length, Math.Min(clockStrings.Length, Math.Min(durationStrings.Length, iwarpStrings.Length)));

		// Initialize controls array
		controls = new SailControl[ncontrols];

		// Populate controls
		for(var i = 0; i < ncontrols; i++) {
		    controls[i] = new SailControl(sail,
						  this,
						  SailControl.ParseSingle(coneStrings[i]),
						  SailControl.ParseSingle(clockStrings[i]),
						  SailControl.ParseDouble(durationStrings[i]),
						  SailControl.ParseInt(iwarpStrings[i]));
		}
	    }
	}
	
	// GUI
	public void GUI() {
	    GUILayout.BeginVertical();

	    // Set the initial time of the sequence
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Start time: " + UT0.ToString());
	    if(GUILayout.Button("Set to Now")) {
		UT0 = Planetarium.GetUniversalTime();
		Update();
	    }
	    GUILayout.EndHorizontal();

	    // Controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("_Cone__|");
	    GUILayout.Label("_Clock_|");
	    GUILayout.Label("_Days__|");
	    GUILayout.Label("__Warp____|");
	    GUILayout.Label("Color");
	    GUILayout.EndHorizontal();

	    int icolor = 0;
	    durationTotal = 0.0;
	    foreach(var control in controls) {

		// Draw individual GUIs
		control.GUILine(colorMap[icolor]);
		
		// Update total duration
		durationTotal += control.duration;

		// Update color index
		icolor++;
		if (icolor == colorMap.Length) {
		    icolor = 0;
		}
	    }
	    
	    // Add/remove control segments
	    GUILayout.BeginHorizontal();
	    if (GUILayout.Button("Add")) { Add(); };
	    if (GUILayout.Button("Remove")) { Remove(); };
	    GUILayout.EndHorizontal();

	    // Final orbit color
	    colorFinal = colorMap[icolor];
	    var cstyle = new GUIStyle();
	    var ctx = new Texture2D(1, 1);
	    cstyle.normal.background = ctx;
	    ctx.SetPixel(1,1,colorFinal);
	    ctx.Apply();
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Final orbit color: ");
	    GUILayout.Label("___", cstyle);
	    GUILayout.EndHorizontal();

	    // Total duration of sequences
	    GUILayout.Label("Total: " + durationTotal + " sec");
	    
	    GUILayout.EndVertical();
	}

	// Add a control
	public void Add () {
	    var newControls = new SailControl[ncontrols + 1];
	    for(var i = 0; i < ncontrols; i++) {
		newControls[i] = controls[i];
	    }
	    newControls[ncontrols] = SailControl.Default(sail, this);
	    controls = newControls;
	    ncontrols++;
	    Update();
	}
	
	// Remove a control
	public void Remove () {
	    if (ncontrols > 1) { // Don't remove last control
		var newControls = new SailControl[ncontrols - 1];
		for (var i = 0; i < ncontrols - 1; i++) {
		    newControls[i] = controls[i];
		}
		controls = newControls;
		ncontrols--;
		Update();
	    }
	}
	
	// Return control at specified UT
	public SailControl Lookup (double UT) {
	    double deltaUT = UT - UT0; // Time past UT0
	    double durationSum = 0.0; // Running sum of durations
	    foreach (var control in controls) {
		// deltaUT between prior and next duration
		if (deltaUT >= durationSum && deltaUT < (durationSum += control.duration)) {
		    return control;
		}
	    }
	    // If none found, return "sail off" control
	    return sailOff;
	}

	// Update the sail's persistant sail control fields
	public void Update () {
	    // Initial time
	    sail.UT0 = UT0;
	    // Controls
	    sail.cones = controls[0].cone_str;
	    sail.clocks = controls[0].clock_str;
	    sail.durations = controls[0].duration_str;
	    sail.iwarps = controls[0].iwarp.ToString();
	    for (var i = 1; i < ncontrols; i++) {
		sail.cones += delimiter + controls[i].cone_str;
		sail.clocks += delimiter + controls[i].clock_str;
		sail.durations += delimiter + controls[i].duration_str;
		sail.iwarps += delimiter + controls[i].iwarp.ToString();
	    }
	    Debug.Log(sail.UT0.ToString());
	    Debug.Log(sail.cones);
	    Debug.Log(sail.clocks);
	    Debug.Log(sail.durations);
	    Debug.Log(sail.iwarps);

	    sail.preview.Calculate();
	}
    }
}
