using System;
using System.Linq;
using UnityEngine;
using PersistentThrust;

namespace SolarSailNavigator {

    public class Control {

	// Fields
	// Cone Angle
	public float cone;
	public string cone_str;
	// Clock angle
	public float clock;
	public string clock_str;
	// Throttle
	public float throttle;
	public string throttle_str;
	// Duration of steering maneuver
	public double duration;
	public string duration_str;
	// Time
	double days;
	string days_str;
	double hours;
	string hours_str;
	// Warp factor
	public int iwarp; // Index of warpLevels array
	public double warp; // Warp factor
	// Color
	public Color color;
	// Controls object this control is attached to
	public Controls controls;
	// Engine this controls
	public PersistentControlled engine;

	// Static fields
	public static float defaultCone = 90f;
	public static float defaultClock = 0f;
	public static float defaultThrottle = 0f;
	public static double SecondsPerDay = 21600.0;
	public static double SecondsPerHour = 3600.0;
	public static double SecondsPerMinute = 60.0;
	public static double HoursPerDay = 6.0;
	public static double defaultDuration = 10 * SecondsPerDay;
	public static double[] warpLevels = { 1, 2, 3, 4, 5, 10, 50, 100, 1000, 10000, 100000 };
	public static int defaultiwarp = 10;
	
	// Cone controls
	public void GUICone () {
	    GUILayout.Label(cone_str, GUILayout.Width(30));
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
	    GUILayout.Label(clock_str, GUILayout.Width(30));
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

	// Throttle controls
	public void GUIThrottle () {
	    GUILayout.Label(throttle_str, GUILayout.Width(30));
	    if (GUILayout.Button("+")) {
		throttle = (float)Math.Round(throttle + 0.05f, 2);
		if (throttle > 1f) {
		    throttle = 1f;
		}
		throttle_str = throttle.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("-")) {
		throttle = (float)Math.Round(throttle - 0.05f, 2);
		if (throttle < 0f) {
		    throttle = 0f;
		}
		throttle_str = throttle.ToString();
		controls.Update();
	    }
	}
	
	// Time controls
	public void GUITime () {

	    // Days
	    GUILayout.Label(days_str, GUILayout.Width(30));
	    // Increase
	    GUILayout.BeginVertical();
	    if (GUILayout.Button("+")) {
		days++;
		days_str = days.ToString();
		duration = SecondsPerDay * days + SecondsPerHour * hours;
		duration_str = duration.ToString();
		controls.Update();
	    }
	    if (GUILayout.Button("+10")) {
		days += 10;
		days_str = days.ToString();
		duration = SecondsPerDay * days + SecondsPerHour * hours;
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
		    duration = SecondsPerDay * days + SecondsPerHour * hours;
		    duration_str = duration.ToString();
		    controls.Update();
		}
	    }
	    if (GUILayout.Button("-10")) {
		if (days >= 10) {
		    days -= 10;
		    days_str = days.ToString();
		    duration = SecondsPerDay * days + SecondsPerHour * hours;
		    duration_str = duration.ToString();
		    controls.Update();
		}
	    }
	    GUILayout.EndVertical();

	    // Hours
	    GUILayout.Label(hours_str, GUILayout.Width(10));
	    // Increase
	    if (GUILayout.Button("+")) {
		hours++;
		if (hours > HoursPerDay) {
		    hours = HoursPerDay;
		}
		hours_str = hours.ToString();
		duration = SecondsPerDay * days + SecondsPerHour * hours;
		duration_str = duration.ToString();
		controls.Update();
	    }
	    // Decrease
	    if (GUILayout.Button("-")) {
		if (hours > 0) {
		    hours--;
		    hours_str = hours.ToString();
		    duration = SecondsPerDay * days + SecondsPerHour * hours;
		    duration_str = duration.ToString();
		    controls.Update();
		}
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
	    GUILayout.Label(" ", cstyle, GUILayout.Width(30));
	}
	
	// GUI line
	public void GUILine (Color color) {
	    GUILayout.BeginHorizontal();

	    GUICone();
	    GUIClock();
	    GUIThrottle();
	    GUITime();
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

	public Control(PersistentControlled engine, Controls controls, float cone, float clock, float throttle, double duration, int iwarp) {
	    // Engine
	    this.engine = engine;
	    this.cone = cone;
	    // Parent controls object
	    this.controls = controls;
	    // Angles
	    cone_str = cone.ToString();
	    this.clock = clock;
	    clock_str = clock.ToString();
	    // Throttle
	    throttle_str = throttle.ToString();
	    this.throttle = throttle;
	    // Time
	    this.duration = duration;
	    this.days = Math.Floor(duration / SecondsPerDay);
	    this.days_str = days.ToString();
	    this.hours = Math.Floor(duration % SecondsPerDay / SecondsPerHour);
	    this.hours_str = this.hours.ToString();
	    this.duration = days * SecondsPerDay + hours * SecondsPerHour;
	    duration_str = duration.ToString();
	    // Warp factor
	    this.iwarp = iwarp;
	    warp = warpLevels[iwarp];
	}

	public static Control Default (PersistentControlled engine, Controls controls) {
	    return new Control (engine, controls, defaultCone, defaultClock, defaultThrottle, defaultDuration, defaultiwarp);
	}
    }

    public class Controls {

	// Fields

	public int ncontrols;
	public Control[] controls;
	public PersistentControlled engine;
	public double UT0;
	public Control engineOff;
	double durationTotal;
	public Color colorFinal;
	public bool showPreview = false;
	public Preview preview;
	public string previewButtonText = "Show Preview";
	
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

	// Give the engine to which this control is for
	public Controls (PersistentControlled engine) {

	    // Assign engine field
	    this.engine = engine;
	    Debug.Log(this.engine.ToString());

	    // Initial time
	    if (engine.UT0 == 0) {
		UT0 = Planetarium.GetUniversalTime();
	    } else {
		UT0 = engine.UT0;
	    }
	    Debug.Log(UT0.ToString());

	    // Off engine control
	    engineOff = Control.Default(engine, this);

	    // If the engine doesn't have saved controls, return default
	    if (String.IsNullOrEmpty(engine.cones) ||
		String.IsNullOrEmpty(engine.clocks) ||
		String.IsNullOrEmpty(engine.throttles) ||
		String.IsNullOrEmpty(engine.durations)) {
		ncontrols = 1;
		controls = new Control[ncontrols];
		controls[0] = Control.Default(engine, this);

	    } else { // Otherwise, parse saved controls

		// Split into arrays
		var coneStrings = engine.cones.Split(delimiter);
		var clockStrings = engine.clocks.Split(delimiter);
		var throttleStrings = engine.throttles.Split(delimiter);
		var durationStrings = engine.durations.Split(delimiter);

		// Find number of controls
		ncontrols = Math.Min(Math.Min(coneStrings.Length, clockStrings.Length), Math.Min(durationStrings.Length, throttleStrings.Length));

		// Initialize controls array
		controls = new Control[ncontrols];

		// Populate controls
		for(var i = 0; i < ncontrols; i++) {
		    controls[i] = new Control(engine,
					      this,
					      Control.ParseSingle(coneStrings[i]),
					      Control.ParseSingle(clockStrings[i]),
					      Control.ParseSingle(throttleStrings[i]),
					      Control.ParseDouble(durationStrings[i]),
					      Control.defaultiwarp);
		}
	    }

	    // Preview
	    preview = new Preview(engine);
	}

	// Convert length in meters to string with bigger units
	string LengthToString(double lengthm) {
	    if (Math.Abs(lengthm) < 1000) {
		return Math.Round(lengthm).ToString() + " m";
	    } else if (Math.Abs(lengthm) < 1000000000) {
		return Math.Round(lengthm / 1000, 3).ToString() + " km";
	    } else {
		return Math.Round(lengthm / 1000000000, 4).ToString() + " Gm";
	    }
	}

	// Convert speed in m/s to string with bigger units
	string SpeedToString(double speedms) {
	    if (Math.Abs(speedms) < 1000) {
		return Math.Round(speedms, 1).ToString() + " m/s";
	    } else {
		return Math.Round(speedms / 1000, 3).ToString() + " km/s";
	    }
	}
	
	// GUI
	public void ControlsGUI(int WindowID) {
	    GUILayout.BeginVertical();

	    // Lock/Unlock attitude
	    engine.IsLocked = GUILayout.Toggle(engine.IsLocked, "Lock Attitude");

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
	    GUILayout.Label("Cone", GUILayout.Width(80));
	    GUILayout.Label("Clock", GUILayout.Width(80));
	    GUILayout.Label("Throttle", GUILayout.Width(80));
	    GUILayout.Label("Days", GUILayout.Width(120));
	    GUILayout.Label("Hours", GUILayout.Width(65));
	    GUILayout.Label("Color", GUILayout.Width(30));
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
	    GUILayout.Label(" ", cstyle);
	    GUILayout.EndHorizontal();

	    // Total duration of sequences
	    GUILayout.Label("Duration: " + durationTotal + " sec");

	    // Final orbit elements
	    if (FlightGlobals.fetch.VesselTarget != null && showPreview && preview.orbitf != null) {
		GUILayout.Label("Final orbit errors:");
		GUILayout.Label("Apoapsis: " + LengthToString(preview.ApErr));
		GUILayout.Label("Periapsis: " + LengthToString(preview.PeErr));
		GUILayout.Label("Orbital Period: " + Math.Round(preview.TPErr, 2) + " sec");
		GUILayout.Label("Inclination: " + Math.Round(preview.IncErr, 3) + " deg");
		GUILayout.Label("Eccentricity: " + Math.Round(preview.EccErr, 3));
		GUILayout.Label("LAN: " + Math.Round(preview.LANErr, 3));
		GUILayout.Label("AOP: " + Math.Round(preview.AOPErr, 3));
	    }
	    
	    // Show difference with target at end of control sequence
	    if (FlightGlobals.fetch.VesselTarget != null && showPreview) {
		// Distance
		GUILayout.Label("Final distance to target: " + LengthToString(preview.targetD));
		
		// Speed
		GUILayout.Label("Final speed to target: " + SpeedToString(preview.targetV));
	    }

	    // Preview orbit
	    if (GUILayout.Button(previewButtonText)) {
	    	if (!showPreview) {
		    showPreview = true;
		    preview.Calculate();
		    previewButtonText = "Hide Preview";
		} else {
		    showPreview = false;
		    preview.Destroy();
		    previewButtonText = "Show Preview";
		}
	    }
	    
	    GUILayout.EndVertical();

	    GUI.DragWindow();
	}

	// Controls GUI rectangle
	private Rect controlWindowPos = new Rect(0, 50, 0, 0);

	// Controls GUI function
	public void DrawControls () {
	    if (engine.vessel == FlightGlobals.ActiveVessel)
		controlWindowPos = GUILayout.Window(10, controlWindowPos, ControlsGUI, "Controls");
	}
	
	// Add a control
	public void Add () {
	    var newControls = new Control[ncontrols + 1];
	    for(var i = 0; i < ncontrols; i++) {
		newControls[i] = controls[i];
	    }
	    newControls[ncontrols] = Control.Default(engine, this);
	    controls = newControls;
	    ncontrols++;
	    colorFinal = colorMap[ncontrols % colorMap.Length];
	    Update();
	}
	
	// Remove a control
	public void Remove () {
	    if (ncontrols > 1) { // Don't remove last control
		var newControls = new Control[ncontrols - 1];
		for (var i = 0; i < ncontrols - 1; i++) {
		    newControls[i] = controls[i];
		}
		controls = newControls;
		ncontrols--;
		colorFinal = colorMap[ncontrols % colorMap.Length];
		Update();
	    }
	}
	
	// Return control at specified UT
	public Control Lookup (double UT) {
	    double deltaUT = UT - UT0; // Time past UT0
	    double durationSum = 0.0; // Running sum of durations
	    foreach (var control in controls) {
		// deltaUT between prior and next duration
		if (deltaUT >= durationSum && deltaUT < (durationSum += control.duration)) {
		    return control;
		}
	    }
	    // If none found, return "engine off" control
	    return engineOff;
	}

	// Update the engine's persistant engine control fields
	public void Update () {
	    // Initial time
	    engine.UT0 = UT0;
	    // Controls
	    engine.cones = controls[0].cone_str;
	    engine.clocks = controls[0].clock_str;
	    engine.throttles = controls[0].throttle_str;
	    engine.durations = controls[0].duration_str;
	    for (var i = 1; i < ncontrols; i++) {
		engine.cones += delimiter + controls[i].cone_str;
		engine.clocks += delimiter + controls[i].clock_str;
		engine.throttles += delimiter + controls[i].throttle_str;
		engine.durations += delimiter + controls[i].duration_str;
	    }
	    preview.Calculate();
	}
    }
}
