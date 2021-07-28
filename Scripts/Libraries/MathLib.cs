using UnityEngine;

namespace GP2_Team7.Libraries
{
	public static class MathLib
	{
		/// <summary>
		/// The first 16 digits of Tau for use in trig functions
		/// </summary>
		private const float TAU = 6.2831853071795864f;
		
		/// <summary>
		/// Lerps a position with a bezier curve shape
		/// </summary>
		/// <param name="origin">The origin position the lerp starts at</param>
		/// <param name="destination"> The destination position the lerp ends at</param>
		/// <param name="relativeOriginTangent">The bezier tangent of the origin position (relative to the origin)</param>
		/// <param name="relativeDestinationTangent">The destination tangent of the destination position (relative to the destination)</param>
		/// <param name="t">The t value of all the internal lerps (the interpolator)</param>
		/// <returns></returns>
		public static Vector3 BezierLerp(Vector3 origin, Vector3 destination, Vector3 relativeOriginTangent, Vector3 relativeDestinationTangent, float t)
		{
			relativeDestinationTangent += destination;
			relativeOriginTangent += origin;
			
			Vector3 a = Vector3.Lerp(origin, relativeOriginTangent, t);
			Vector3 b = Vector3.Lerp(relativeOriginTangent, relativeDestinationTangent, t);
			Vector3 c = Vector3.Lerp(relativeDestinationTangent, destination, t);
			Vector3 d = Vector3.Lerp(a, b, t);
			Vector3 e = Vector3.Lerp(b, c, t);
			return Vector3.Lerp(d, e, t);
		}

		/// <summary>
		/// Remaps a value within a specified input range to a specified output range
		/// </summary>
		/// <param name="minI">Minimum input range</param>
		/// <param name="maxI">Maximum input range</param>
		/// <param name="minO">Minimum output range</param>
		/// <param name="maxO">Maximum output range</param>
		/// <param name="value">Input value to remap</param>
		/// <param name="clamped">If the method should be clamped or not, defaults to true</param>
		/// <returns>Remapped output</returns>
		public static float Remap(float minI, float maxI, float minO, float maxO, float value, bool clamped = true)
		{
			float t = Mathf.InverseLerp(minI, maxI, value);
			if (clamped) t = Mathf.Clamp01(t);
			return Mathf.Lerp(minO, maxO, t);
		}
	}
}
    
