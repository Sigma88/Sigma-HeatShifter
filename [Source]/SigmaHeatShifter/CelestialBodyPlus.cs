using System;
using UnityEngine;


namespace SigmaHeatShifterPlugin
{
    internal class CelestialBodyPlus : CelestialBody
    {
        public override void GetAtmoThermalStats(bool getBodyFlux, CelestialBody sunBody, Vector3d sunVector, double sunDot, Vector3d upAxis, double altitude, out double atmosphereTemperatureOffset, out double bodyEmissiveFlux, out double bodyAlbedoFlux)
        {
            atmosphereTemperatureOffset = 0.0;
            bodyEmissiveFlux = 0.0;
            bodyAlbedoFlux = 0.0;
            if (sunBody == this)
            {
                return;
            }
            Vector3d a = sunBody.scaledBody.transform.position;
            Vector3 up = bodyTransform.up;
            double num = (double)Vector3.Dot(sunVector, up);
            double num2 = (double)Vector3.Dot(up, upAxis);
            double num3 = Math.Acos(num2);
            if (double.IsNaN(num3))
            {
                double num4;
                if (!(num2 < 0.0))
                {
                    num4 = 0.0;
                }
                else
                {
                    num4 = 3.1415926535897931;
                }
                num3 = num4;
            }
            double num5 = Math.Acos(num);
            if (double.IsNaN(num5))
            {
                double num6;
                if (!(num < 0.0))
                {
                    num6 = 0.0;
                }
                else
                {
                    num6 = 3.1415926535897931;
                }
                num5 = num6;
            }
            double num7 = (1.0 + Math.Cos(num5 - num3)) * 0.5;
            double num8 = (1.0 + Math.Cos(num5 + num3)) * 0.5;
            if (num2 < 0.0)
            {
                num = 0.0 - num;
            }
            double num9 = num3;
            double num10 = num5;
            if (num3 > 1.5707963267948966)
            {
                num9 = 3.1415926535897931 - num9;
                num10 = 3.1415926535897931 - num10;
            }
            double sqrMagnitude = ((a - scaledBody.transform.position) * (double)ScaledSpace.ScaleFactor).sqrMagnitude;
            double num11 = PhysicsGlobals.SolarLuminosity / (12.566370614359172 * sqrMagnitude);
            double num12 = (1.0 + (double)Vector3.Dot(sunVector, Quaternion.AngleAxis(this.maxTempAngleOffset() * Mathf.Sign((float)rotationPeriod), up) * (Vector3)upAxis)) * 0.5;
            double num13 = num7 - num8;
            double num14;
            if (num13 > 0.001)
            {
                num14 = (num12 - num8) / num13;
                if (double.IsNaN(num14))
                {
                    if (num12 > 0.5)
                    {
                        num14 = 1.0;
                    }
                    else
                    {
                        num14 = 0.0;
                    }
                }
            }
            else
            {
                num14 = num8 + num13 * 0.5;
            }
            if (atmosphere)
            {
                float num15 = (float)(1.5707963267948966 - num9);
                num15 *= 57.29578f;
                CelestialBody bodyReferencing = GetBodyReferencing(this, sunBody);
                float time = ((float)bodyReferencing.orbit.trueAnomaly * 57.29578f + 360f) % 360f;
                double num16 = (double)latitudeTemperatureBiasCurve.Evaluate(num15) + (double)latitudeTemperatureSunMultCurve.Evaluate(num15) * num14 + (double)(axialTemperatureSunBiasCurve.Evaluate(time) * axialTemperatureSunMultCurve.Evaluate(num15));
                double num17;
                if (bodyReferencing.orbit.eccentricity != 0.0)
                {
                    num17 = (double)eccentricityTemperatureBiasCurve.Evaluate((float)((bodyReferencing.orbit.radius - bodyReferencing.orbit.PeR) / (bodyReferencing.orbit.ApR - bodyReferencing.orbit.PeR)));
                }
                else
                {
                    num17 = 0.0;
                }
                atmosphereTemperatureOffset = num16 + num17;
            }
            else
            {
                atmosphereTemperatureOffset = 0.0;
            }
            if (!getBodyFlux)
            {
                return;
            }
            double num18 = 0.0;
            double num19 = 0.0;
            double num20 = 0.0;
            double a2;
            if (atmosphere)
            {
                double temperature = GetTemperature(0.0);
                a2 = temperature + atmosphereTemperatureOffset;
                num19 = temperature + (double)(latitudeTemperatureBiasCurve.Evaluate(90f) + axialTemperatureSunMultCurve.Evaluate(0f - (float)orbit.inclination));
                num20 = temperature + (double)(latitudeTemperatureBiasCurve.Evaluate(0f) + latitudeTemperatureSunMultCurve.Evaluate(0f) + axialTemperatureSunMultCurve.Evaluate((float)orbit.inclination));
                num18 = 1.0 - Math.Sqrt(num20) * 0.0016;
                num18 = UtilMath.Clamp01(num18);
            }
            else
            {
                double spaceTemperature = PhysicsGlobals.SpaceTemperature;
                spaceTemperature *= spaceTemperature;
                spaceTemperature *= spaceTemperature;
                double num21 = 1.0 / (PhysicsGlobals.StefanBoltzmanConstant * emissivity);
                double num22 = num11 * (1.0 - albedo) * num21;
                double num23 = Math.Sqrt(Math.Sqrt(0.25 * num22 + spaceTemperature));
                double num24 = Math.Sqrt(Math.Sqrt(num22 + spaceTemperature)) - num23;
                num20 = num23 + Math.Sqrt(num24) * 2.0;
                num19 = num23 - Math.Pow(num24, 1.1) * 1.22;
                double t = 2.0 / Math.Sqrt(Math.Sqrt(solarDayLength));
                num20 = UtilMath.Lerp(num20, num23, t);
                num19 = UtilMath.Lerp(num19, num23, t);
                double d = Math.Max(0.0, num7 * 2.0 - 1.0);
                d = Math.Sqrt(d);
                num18 = 1.0 - Math.Sqrt(num20) * 0.0016;
                num18 = UtilMath.Clamp01(num18);
                double num25 = (num20 - num19) * d;
                double num26 = num19 + num25;
                double num27 = num19 + num25 * num18;
                a2 = Math.Max(PhysicsGlobals.SpaceTemperature, num27 + (num26 - num27) * num14 + coreTemperatureOffset);
            }
            double b = UtilMath.LerpUnclamped(UtilMath.LerpUnclamped(UtilMath.LerpUnclamped(num19, num20, UtilMath.LerpUnclamped(0.782048841, 0.87513007, num18)), UtilMath.LerpUnclamped(num19, num20, UtilMath.LerpUnclamped(0.093081228, 0.87513007, num18)), num14), UtilMath.LerpUnclamped(num19, num20, UtilMath.LerpUnclamped(0.398806364, 0.797612728, num18)), num7);
            double num28 = UtilMath.Lerp(a2, b, 0.2 + altitude / Radius * 0.5);
            num28 *= num28;
            num28 *= num28;
            double num29 = Radius * Radius / ((Radius + altitude) * (Radius + altitude));
            if (num29 > 1.0)
            {
                num29 = 1.0;
            }
            bodyEmissiveFlux = PhysicsGlobals.StefanBoltzmanConstant * emissivity * num28 * num29;
            bodyAlbedoFlux = num11 * 0.5 * (sunDot + 1.0) * albedo * num29;
        }
    }
}
