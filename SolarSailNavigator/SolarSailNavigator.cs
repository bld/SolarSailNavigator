using System;
using System.Linq;
using UnityEngine;

namespace SolarSailNavigator {

    public class ModuleSolarSail : PartModule {
	// Persistent Variables
	// Sail enabled
	[KSPField(isPersistant = true)]
	public bool IsEnabled = false;
	// Attitude locked
	[KSPField(isPersistant = true)]
	public bool IsLocked = false;
	// Cone angle
	[KSPField(isPersistant = true)]
	protected float coneAngle_f = 0;
	protected string coneAngle = "";
	// Clock angle
	[KSPField(isPersistant = true)]
	protected float clockAngle_f = 0;
	protected string clockAngle = "";
	// Preview seconds
	[KSPField(isPersistant = true)]
	protected string previewSeconds_str = KSPUtil.KerbinYear.ToString();
	// Preview warp factor
	[KSPField(isPersistant = true)]
	protected string previewCurrentRate_str = "1";
	// Preview orbit
	protected Orbit previewOrbit0;
	protected Orbit previewOrbitf;
	protected Orbit[] previewOrbits;
	protected LineRenderer previewOrbitLine;
	protected double UT0;
	protected double UTf;
	
	// Persistent False
	[KSPField]
	public float reflectedPhotonRatio = 1f;
	[KSPField]
	public float surfaceArea; // Surface area of the panel.
	[KSPField]
	public string animName;
	
	// GUI
	// Force and Acceleration
	[KSPField(guiActive = true, guiName = "Force")]
	protected string forceAcquired = "";
	[KSPField(guiActive = true, guiName = "Acceleration")]
	protected string solarAcc = "";
	
	protected Transform surfaceTransform = null;
	protected Animation solarSailAnim = null;
	
	const double kerbin_distance = 13599840256;
	const double thrust_coeff = 9.08e-6;
	
	protected double solar_force_d = 0;
	protected double solar_acc_d = 0;
	protected long count = 0;

	[KSPEvent(guiActive = true, guiName = "Deploy Sail", active = true)]
	public void DeploySail() {
	    if (animName != null && solarSailAnim != null) {
		solarSailAnim[animName].speed = 1f;
		solarSailAnim[animName].normalizedTime = 0f;
		solarSailAnim.Blend(animName, 2f);
	    }
	    IsEnabled = true;
	    // Create control window
	    RenderingManager.AddToPostDrawQueue(3, new Callback(DrawControls));
	}
	
	[KSPEvent(guiActive = true, guiName = "Retract Sail", active = false)]
	public void RetractSail() {
	    if (animName != null && solarSailAnim != null) {
		solarSailAnim[animName].speed = -1f;
		solarSailAnim[animName].normalizedTime = 1f;
		solarSailAnim.Blend(animName, 2f);
	    }
	    IsEnabled = false;
	    // Remove control window
	    RenderingManager.RemoveFromPostDrawQueue(3, new Callback(DrawControls));
	}

	// Tilt controls

	public void TiltPlus() {
	    coneAngle_f += 5;
	    if (coneAngle_f > 90) {
		coneAngle_f = coneAngle_f - 180;
	    }
	}

	public void TiltMinus() {
	    coneAngle_f -= 5;
	    if (coneAngle_f < -90) {
		coneAngle_f = coneAngle_f + 180;
	    }
	}

	// Rotate controls

	public void RotatePlus() {
	    clockAngle_f += 5;
	    if (clockAngle_f > 180) {
		clockAngle_f = clockAngle_f - 360;
	    }
	}

	public void RotateMinus() {
	    clockAngle_f -= 5;
	    if (clockAngle_f < -180) {
		clockAngle_f = 360 + clockAngle_f;
	    }
	}

	// Initialization
	public override void OnStart(StartState state) {
	    if (state != StartState.None && state != StartState.Editor) {
		if (animName != null) {
		    solarSailAnim = part.FindModelAnimators(animName).FirstOrDefault();
		}
		if (IsEnabled) {
		    solarSailAnim[animName].speed = 1f;
		    solarSailAnim[animName].normalizedTime = 0f;
		    solarSailAnim.Blend(animName, 0.1f);
		    RenderingManager.AddToPostDrawQueue(3, new Callback(DrawControls));
		}

		this.part.force_activate();
	    }
	}

	public override void OnUpdate() {
	    // Sail deployment GUI
	    Events["DeploySail"].active = !IsEnabled;
	    Events["RetractSail"].active = IsEnabled;
	    // Text fields (acc & force)
	    Fields["solarAcc"].guiActive = IsEnabled;
	    Fields["forceAcquired"].guiActive = IsEnabled;
	    forceAcquired = solar_force_d.ToString("E") + " N";
	    solarAcc = solar_acc_d.ToString("E") + " m/s";
	    // Tilt/rotate control strings
	    coneAngle = coneAngle_f.ToString() + " deg";
	    clockAngle = clockAngle_f.ToString() + " deg";
	}
	
	public override void OnFixedUpdate() {
	    if (FlightGlobals.fetch != null) {
		
		double UT = Planetarium.GetUniversalTime();
		double dT = TimeWarp.fixedDeltaTime;
		
		solar_force_d = 0;
		if (!IsEnabled) { return; }
		
		// Force attitude to sail frame
		if (IsLocked) {
		    vessel.SetRotation(SailFrame(vessel.orbit, coneAngle_f, clockAngle_f, UT));
		}

		double sunlightFactor = 1.0;

		if (!inSun(vessel.orbit, UT)) {
		    sunlightFactor = 0.0;
		}

		Vector3d solarForce = CalculateSolarForce(this, vessel.orbit, this.part.transform.up, UT) * sunlightFactor;

		Vector3d solarAccel = solarForce / vessel.GetTotalMass() / 1000.0;

		if (!this.vessel.packed) {
		    vessel.ChangeWorldVelocity(solarAccel * dT);
		} else {
		    PerturbOrbit(vessel.orbit, solarAccel, UT, dT);
		}

		solar_force_d = solarForce.magnitude;
		solar_acc_d = solarAccel.magnitude;
	    }
	    count++;

	    // Update preview orbit if it exists
	    UpdatePreview();
	}
		
	public static Quaternion RTNFrame (Vessel vessel) {
	    // Center of mass position
	    var CM = vessel.findWorldCenterOfMass();
	    // Unit position vector
	    var r = (CM - vessel.mainBody.position).normalized;
	    // Unit velocity vector
	    var v = vessel.obt_velocity.normalized;
	    // Unit orbital angular velocity
	    var h = Vector3d.Cross(r, v).normalized;
	    // Tangential vector
	    var t = Vector3d.Cross(h, r).normalized;
	    // Quaternion of RTN frame
	    return Quaternion.LookRotation(t, r);
	}

	public static Quaternion SailFrame (Vessel vessel, float cone, float clock) {
	    var QRTN = RTNFrame(vessel);
	    var QCC = Quaternion.Euler(0, 90 - clock, cone);
	    return QRTN * QCC;
	}
	
	private Rect controlWindowPos = new Rect();

	private void DrawControls () {
	    if (this.vessel == FlightGlobals.ActiveVessel)
		controlWindowPos = GUILayout.Window(10, controlWindowPos, SailControlsGUI, "Sail Controls");
	}

	private void SailControlsGUI (int WindowID) {

	    GUILayout.BeginVertical();

	    // Lock/Unlock attitude
	    IsLocked = GUILayout.Toggle(IsLocked, "Lock Attitude");

	    // Cone angle controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Cone angle");
	    GUILayout.Label(coneAngle);
	    if (GUILayout.Button("+")) {
		TiltPlus();
	    }
	    if (GUILayout.Button("-")) {
		TiltMinus();
	    }
	    GUILayout.EndHorizontal();
	    
	    // Clock angle controls
	    GUILayout.BeginHorizontal();
	    GUILayout.Label("Clock angle");
	    GUILayout.Label(clockAngle);
	    if (GUILayout.Button("+")) {
		RotatePlus();
	    }
	    if (GUILayout.Button("-")) {
		RotateMinus();
	    }
	    GUILayout.EndHorizontal();

	    // Preview orbit
	    GUILayout.Label("Preview Orbit");
	    GUILayout.Label("Seconds ahead:");
	    previewSeconds_str = GUILayout.TextField(previewSeconds_str, 25);
	    GUILayout.Label("Warp factor:");
	    previewCurrentRate_str = GUILayout.TextField(previewCurrentRate_str, 25);
	    if (GUILayout.Button("Preview Orbit")) {
		Debug.Log("Preview");
		PreviewOrbit();
	    }

	    if (previewOrbitf != null) {
		GUILayout.Label("i: " + previewOrbitf.inclination.ToString());
		GUILayout.Label("e: " + previewOrbitf.eccentricity.ToString());
		GUILayout.Label("SMA: " + previewOrbitf.semiMajorAxis.ToString());
		GUILayout.Label("LAN: " + previewOrbitf.LAN.ToString());
		GUILayout.Label("AoP: " + previewOrbitf.argumentOfPeriapsis.ToString());
		GUILayout.Label("mAaE: " + previewOrbitf.meanAnomalyAtEpoch.ToString());
	    }

	    // Debugging stuff
	    if (GUILayout.Button("Debuginfo")) {
		Debug.Log("ScaledSpace.ScaleFactor: " + ScaledSpace.ScaleFactor.ToString());
	    }
	    
	    GUILayout.EndVertical();
	    
	    GUI.DragWindow();
	}

	// Calculate RTN frame quaternion given an orbit and UT
	public static Quaternion RTNFrame (Orbit orbit, double UT) {
	    // Position
	    var r = transposeYZ(orbit.getRelativePositionAtUT(UT).normalized);
	    // Velocity
	    var v = transposeYZ(orbit.getOrbitalVelocityAtUT(UT).normalized);
	    // Unit orbit angular momentum
	    var h = Vector3d.Cross(r, v).normalized;
	    // Tangential
	    var t = Vector3d.Cross(h, r).normalized;
	    // QRTN
	    return Quaternion.LookRotation(t, r);
	}

	// Sail frame in local (RTN) coordinates
	public static Quaternion SailFrameLocal (float cone, float clock) {
	    return Quaternion.Euler(0, 90 - clock, cone);
	}
	
	// Sail frame given an orbit, angles, and UT
	public static Quaternion SailFrame (Orbit orbit, float cone, float clock, double UT) {
	    var QRTN = RTNFrame(orbit, UT);
	    var QCC = SailFrameLocal(cone, clock);
	    return QRTN * QCC;
	}

	// Test if an orbit at UT is in sunlight
	public static bool inSun(Orbit orbit, double UT) {
	    Vector3d a = orbit.getPositionAtUT(UT);
	    Vector3d b = FlightGlobals.Bodies[0].getPositionAtUT(UT);
	    foreach (CelestialBody referenceBody in FlightGlobals.Bodies) {
		if (referenceBody.flightGlobalsIndex == 0) { // the sun should not block line of sight to the sun
		    continue;
		}
		Vector3d refminusa = referenceBody.getPositionAtUT(UT) - a;
		Vector3d bminusa = b - a;
		if (Vector3d.Dot(refminusa, bminusa) > 0) {
		    if (Vector3d.Dot(refminusa, bminusa.normalized) < bminusa.magnitude) {
			Vector3d tang = refminusa - Vector3d.Dot(refminusa, bminusa.normalized) * bminusa.normalized;
			if (tang.magnitude < referenceBody.Radius) {
			    return false;
			}
		    }
		}
	    }
	    return true;
	}

	private static double solarForceAtDistance(Vector3d sunPosition, Vector3d ownPosition) {
	    double distance_from_sun = Vector3.Distance(sunPosition, ownPosition);
	    double force_to_return = thrust_coeff * kerbin_distance * kerbin_distance / distance_from_sun / distance_from_sun;
	    return force_to_return;
	}
	
	// Calculate solar force as function of
	// sail, orbit, transform, and UT
	public static Vector3d CalculateSolarForce(ModuleSolarSail sail, Orbit orbit, Vector3d normal, double UT) {
	    if (sail.part != null) {
		Vector3d sunPosition = FlightGlobals.Bodies[0].getPositionAtUT(UT);
		Vector3d ownPosition = orbit.getPositionAtUT(UT);
		Vector3d ownsunPosition = ownPosition - sunPosition;
		// If normal points away from sun, negate so our force is always away from the sun
		// so that turning the backside towards the sun thrusts correctly
		if (Vector3d.Dot (normal, ownsunPosition) < 0) {
		    normal = -normal;
		}
		// Magnitude of force proportional to cosine-squared of angle between sun-line and normal
		double cosConeAngle = Vector3.Dot (ownsunPosition.normalized, normal);
		
		Vector3d force = normal * cosConeAngle * cosConeAngle * sail.surfaceArea * sail.reflectedPhotonRatio * solarForceAtDistance(sunPosition, ownPosition);
		return force;
	    } else {
		return Vector3d.zero;
	    }
	}

	// Perturn an orbit by an acceleration at UT with time step dT
	public static void PerturbOrbit (Orbit orbit, Vector3d accel, double UT, double dT) {

	    // Return updated orbit if in sun
	    if (accel.magnitude > 0) {
		// Transpose Y and Z for Orbit class
		Vector3d accel_orbit = new Vector3d(accel.x, accel.z, accel.y);
		Vector3d position = orbit.getRelativePositionAtUT(UT);
		Orbit orbit2 = CloneOrbit(orbit);
		orbit2.UpdateFromStateVectors(position, orbit.getOrbitalVelocityAtUT(UT) + accel_orbit * dT, orbit.referenceBody, UT);
		if (!double.IsNaN(orbit2.inclination) && !double.IsNaN(orbit2.eccentricity) && !double.IsNaN(orbit2.semiMajorAxis) && orbit2.timeToAp > dT) {
		    orbit.inclination = orbit2.inclination;
		    orbit.eccentricity = orbit2.eccentricity;
		    orbit.semiMajorAxis = orbit2.semiMajorAxis;
		    orbit.LAN = orbit2.LAN;
		    orbit.argumentOfPeriapsis = orbit2.argumentOfPeriapsis;
		    orbit.meanAnomalyAtEpoch = orbit2.meanAnomalyAtEpoch;
		    orbit.epoch = orbit2.epoch;
		    orbit.referenceBody = orbit2.referenceBody;
		    orbit.Init();
		    orbit.UpdateFromUT(UT);
		}
	    }
	}

	// Transpose X and Y elements for conversion of Orbit vector3d
	public static Vector3d transposeYZ (Vector3d v) {
	    return new Vector3d(v.x, v.z, v.y);
	}

	// Dublicate an orbit
	public static Orbit CloneOrbit(Orbit orbit0) {
	    return new Orbit(orbit0.inclination, orbit0.eccentricity, orbit0.semiMajorAxis, orbit0.LAN, orbit0.argumentOfPeriapsis, orbit0.meanAnomalyAtEpoch, orbit0.epoch, orbit0.referenceBody);
	}
	
	// Preview current state in new orbit
	public void PreviewOrbit() {
	    UT0 = Planetarium.GetUniversalTime();
	    Debug.Log(UT0.ToString());
	    UTf = UT0 + Convert.ToDouble(previewSeconds_str);
	    Debug.Log(UTf.ToString());
	    double dT = TimeWarp.fixedDeltaTime * Convert.ToDouble(previewCurrentRate_str);
	    Debug.Log(dT.ToString());
	    // Calculate preview orbit
	    previewOrbits = PropagateOrbit(this, vessel.orbit, UT0, UTf, dT, coneAngle_f, clockAngle_f, vessel.GetTotalMass());
	    previewOrbit0 = previewOrbits[0];
	    previewOrbitf = previewOrbits[previewOrbits.Length - 1];
	    // Draw preview orbit
	    if (previewOrbitLine != null) {
		Destroy(previewOrbitLine);
	    }
	    GameObject previewObject = new GameObject("Preview Orbit");
	    previewOrbitLine = previewObject.AddComponent<LineRenderer>();
	    previewOrbitLine.useWorldSpace = false;
	    previewObject.layer = 10;
	    previewOrbitLine.material = MapView.fetch.orbitLinesMaterial;
	    previewOrbitLine.SetColors(Color.yellow, Color.yellow);
	    previewOrbitLine.SetWidth(10000, 10000);
	    previewOrbitLine.SetVertexCount(previewOrbits.Length);
	}

	public void UpdatePreview() {
	    if (previewOrbitLine != null) {
		// Enable only on map
		if (MapView.MapIsEnabled) {
		    previewOrbitLine.enabled = true;
		    // Update points
		    Vector3d rRefUT0 = vessel.orbit.referenceBody.getPositionAtUT(UT0);
		    for (int i = 0; i < previewOrbits.Length; i++) {
			double UTi = previewOrbits[i].epoch;
			previewOrbitLine.SetPosition(i, ScaledSpace.LocalToScaledSpace(previewOrbits[i].getRelativePositionAtUT(UTi).xzy + rRefUT0));
		    }
		} else {
		    previewOrbitLine.enabled = false;
		}
	    }
	}

	// Propagate an orbit
	public static Orbit[] PropagateOrbit (ModuleSolarSail sail, Orbit orbit0, double UT0, double UTf, double dT, float cone, float clock, double mass) {
	    Orbit orbit = CloneOrbit(orbit0);

	    int nsteps = Convert.ToInt32(Math.Ceiling((UTf - UT0) / dT));
	    Debug.Log("nsteps: " + nsteps.ToString());
	    double dTlast = (UTf - UT0) % dT;
	    Debug.Log("dTlast: " + dTlast.ToString());

	    double UT;

	    var orbits = new Orbit[1 + nsteps];
	    orbits[0] = CloneOrbit(orbit0);
	    
	    for (int i = 0; i < nsteps; i++) {
		// Last step goes to UTf
		if (i == nsteps - 1) {
		    dT = dTlast;
		    UT = UTf;
		} else {
		    UT = UT0 + i * dT;
		}

		double sunlightFactor = 1.0;
		if(!inSun(orbit, UT)) {
		    sunlightFactor = 0.0;
		}

		Quaternion sailFrame = SailFrame(orbit, cone, clock, UT);

		Vector3d normal = sailFrame * new Vector3d(0, 1, 0);

		Vector3d solarForce = CalculateSolarForce(sail, orbit, normal, UT) * sunlightFactor;

		Vector3d solarAccel = solarForce / mass / 1000.0;
		
		PerturbOrbit(orbit, solarAccel, UT, dT);

		orbits[1 + i] = CloneOrbit(orbit);
		
		Debug.Log("i: " + i.ToString() +
			  ", UT: " + UT.ToString() +
			  ", dT: " + dT.ToString() +
			  ", UTf-UT: " + (UTf-UT).ToString() +
			  ", ApA: " + orbit.ApA.ToString() +
			  ", PeA: " + orbit.PeA.ToString() +
			  ", epoch: " + orbit.epoch.ToString());
	    }
	    
	    // Return propagated orbit
	    return orbits;
	}
    }
}
