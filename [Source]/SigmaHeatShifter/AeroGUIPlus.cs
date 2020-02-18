using System;
using UnityEngine;


namespace SigmaHeatShifterPlugin
{
    internal class AeroGUIPlus : AeroGUI
    {
        private const double DEG2RAD = 0.017453292519943295;
        private const double RAD2DEG = 57.295779513082323;
        private const double bodyEmissiveScalarS0Front = 0.782048841;
        private const double bodyEmissiveScalarS0Back = 0.093081228;
        private const double bodyEmissiveScalarS1 = 0.87513007;
        private const double bodyEmissiveScalarS0Top = 0.398806364;
        private const double bodyEmissiveScalarS1Top = 0.797612728;
        private double solarFlux;
        private double backgroundRadTemp;
        private double bodyAlbedoFlux;
        private double bodyEmissiveFlux;
        private double bodySunFlux;
        private double effectiveFaceTemp;
        private double bodyTemperature;
        private double sunDot;
        private double atmosphereTemperatureOffset;
        private double altTempMult;
        private double latitude;
        private double latTempMod;
        private double axialTempMod;
        private double solarAMMult;
        private double finalAtmoMod;
        private double sunFinalMult;
        private double diurnalRange;

        public void GetThermalStatsPlus(Vessel vessel)
        {
            FlightIntegrator component = vessel.GetComponent<FlightIntegrator>();
            if (component == null)
            {
                return;
            }
            shockTemp = vessel.externalTemperature;
            backgroundRadTemp = component.backgroundRadiationTemp;
            double t = component.CalculateDensityThermalLerp();
            Vector3 lhs = (Planetarium.fetch.Sun.scaledBody.transform.position - ScaledSpace.LocalToScaledSpace(vessel.transform.position)).normalized;
            solarFlux = vessel.solarFlux;
            if (Planetarium.fetch.Sun == vessel.mainBody)
            {
                return;
            }
            Vector3 vector = (Planetarium.fetch.Sun.scaledBody.transform.position - vessel.mainBody.scaledBody.transform.position) * ScaledSpace.ScaleFactor;
            double num = vector.sqrMagnitude;
            bodySunFlux = PhysicsGlobals.SolarLuminosity / (12.566370614359172 * num);
            sunDot = Vector3.Dot(lhs, vessel.upAxis);
            Vector3 up = vessel.mainBody.bodyTransform.up;
            float num2 = Vector3.Dot(lhs, up);
            double num3 = Vector3.Dot(up, vessel.upAxis);
            double num4 = Math.Acos(num3);
            if (double.IsNaN(num4))
            {
                double num5;
                if (!(num3 < 0.0))
                {
                    num5 = 0.0;
                }
                else
                {
                    num5 = 3.1415926535897931;
                }
                num4 = num5;
            }
            double num6 = Math.Acos(num2);
            if (double.IsNaN(num6))
            {
                double num7;
                if (!(num2 < 0.0))
                {
                    num7 = 0.0;
                }
                else
                {
                    num7 = 3.1415926535897931;
                }
                num6 = num7;
            }
            double num8 = (1.0 + Math.Cos(num6 - num4)) * 0.5;
            double num9 = (1.0 + Math.Cos(num6 + num4)) * 0.5;
            if (num3 < 0.0)
            {
                num2 = 0f - num2;
            }
            double num10 = num4;
            double num11 = num6;
            if (num4 > 1.5707963267948966)
            {
                num10 = 3.1415926535897931 - num10;
                num11 = 3.1415926535897931 - num11;
            }
            double num12 = (1.0 + Vector3.Dot(lhs, Quaternion.AngleAxis(-(vessel.mainBody.maxTempAngleOffset()) * Mathf.Sign((float)vessel.mainBody.rotationPeriod), up) * vessel.upAxis)) * 0.5;
            double num13 = (num12 - num9) / (num8 - num9);
            if (double.IsNaN(num13))
            {
                if (num12 > 0.5)
                {
                    num13 = 1.0;
                }
                else
                {
                    num13 = 0.0;
                }
            }
            latitude = 1.5707963267948966 - num10;
            latitude *= 57.295779513082323;
            if (vessel.mainBody.atmosphere)
            {
                float time = (float)latitude;
                CelestialBody bodyReferencing = CelestialBody.GetBodyReferencing(vessel.mainBody, FlightIntegrator.sunBody);
                diurnalRange = vessel.mainBody.latitudeTemperatureSunMultCurve.Evaluate(time);
                latTempMod = vessel.mainBody.latitudeTemperatureBiasCurve.Evaluate(time);
                float time2 = ((float)bodyReferencing.orbit.trueAnomaly * 57.29578f + 360f) % 360f;
                axialTempMod = (vessel.mainBody.axialTemperatureSunBiasCurve.Evaluate(time2) * vessel.mainBody.axialTemperatureSunMultCurve.Evaluate(time));
                atmosphereTemperatureOffset = latTempMod + diurnalRange * num13 + axialTempMod + vessel.mainBody.eccentricityTemperatureBiasCurve.Evaluate((float)((bodyReferencing.orbit.radius - bodyReferencing.orbit.PeR) / (bodyReferencing.orbit.ApR - bodyReferencing.orbit.PeR)));
                altTempMult = vessel.mainBody.atmosphereTemperatureSunMultCurve.Evaluate((float)vessel.altitude);
                if (vessel.atmDensity > 0.0)
                {
                    finalAtmoMod = atmosphereTemperatureOffset * altTempMult;
                    double num14 = vessel.mainBody.radiusAtmoFactor * sunDot;
                    if (num14 < 0.0)
                    {
                        solarAMMult = Math.Sqrt(2.0 * vessel.mainBody.radiusAtmoFactor + 1.0);
                    }
                    else
                    {
                        solarAMMult = Math.Sqrt(num14 * num14 + 2.0 * vessel.mainBody.radiusAtmoFactor + 1.0) - num14;
                    }
                    sunFinalMult = vessel.mainBody.GetSolarPowerFactor(vessel.atmDensity * solarAMMult);
                    double num15 = (vessel.mach - PhysicsGlobals.NewtonianMachTempLerpStartMach) / (PhysicsGlobals.NewtonianMachTempLerpEndMach - PhysicsGlobals.NewtonianMachTempLerpStartMach);
                    if (num15 > 0.0)
                    {
                        num15 = Math.Pow(num15, PhysicsGlobals.NewtonianMachTempLerpExponent);
                        num15 = Math.Min(1.0, num15);
                        double b = Math.Pow(0.5 * vessel.convectiveMachFlux / (PhysicsGlobals.StefanBoltzmanConstant * PhysicsGlobals.RadiationFactor), 0.25);
                        shockTemp = Math.Max(shockTemp, UtilMath.LerpUnclamped(shockTemp, b, num15));
                    }
                }
            }
            double num16 = 0.0;
            double num17 = 0.0;
            double num18 = 0.0;
            if (vessel.mainBody.atmosphere)
            {
                double temperature = vessel.mainBody.GetTemperature(0.0);
                bodyTemperature = temperature + atmosphereTemperatureOffset;
                num17 = temperature + (vessel.mainBody.latitudeTemperatureBiasCurve.Evaluate(90f) + vessel.mainBody.axialTemperatureSunMultCurve.Evaluate(0f - (float)vessel.mainBody.orbit.inclination));
                num18 = temperature + (vessel.mainBody.latitudeTemperatureBiasCurve.Evaluate(0f) + vessel.mainBody.latitudeTemperatureSunMultCurve.Evaluate(0f) + vessel.mainBody.axialTemperatureSunMultCurve.Evaluate((float)vessel.mainBody.orbit.inclination));
                num16 = 1.0 - Math.Sqrt(num18) * 0.0016;
                num16 = UtilMath.Clamp01(num16);
            }
            else
            {
                double spaceTemperature = PhysicsGlobals.SpaceTemperature;
                spaceTemperature *= spaceTemperature;
                spaceTemperature *= spaceTemperature;
                double num19 = 1.0 / (PhysicsGlobals.StefanBoltzmanConstant * vessel.mainBody.emissivity);
                double num20 = bodySunFlux * (1.0 - vessel.mainBody.albedo) * num19;
                double num21 = Math.Pow(0.25 * num20 + spaceTemperature, 0.25);
                double num22 = Math.Pow(num20 + spaceTemperature, 0.25) - num21;
                num18 = num21 + Math.Sqrt(num22) * 2.0;
                num17 = num21 - Math.Pow(num22, 1.1) * 1.22;
                double t2 = 2.0 / Math.Sqrt(Math.Sqrt(vessel.mainBody.solarDayLength));
                num18 = UtilMath.Lerp(num18, num21, t2);
                num17 = UtilMath.Lerp(num17, num21, t2);
                double d = Math.Max(0.0, num8 * 2.0 - 1.0);
                d = Math.Sqrt(d);
                num16 = 1.0 - Math.Sqrt(num18) * 0.0016;
                num16 = UtilMath.Clamp01(num16);
                double num23 = (num18 - num17) * d;
                double num24 = num17 + num23;
                double num25 = num17 + num23 * num16;
                bodyTemperature = Math.Max(PhysicsGlobals.SpaceTemperature, num25 + (num24 - num25) * num13 + vessel.mainBody.coreTemperatureOffset);
            }
            effectiveFaceTemp = UtilMath.LerpUnclamped(UtilMath.LerpUnclamped(UtilMath.LerpUnclamped(num17, num18, UtilMath.LerpUnclamped(0.782048841, 0.87513007, num16)), UtilMath.LerpUnclamped(num17, num18, UtilMath.LerpUnclamped(0.093081228, 0.87513007, num16)), num13), UtilMath.LerpUnclamped(num17, num18, UtilMath.LerpUnclamped(0.398806364, 0.797612728, num16)), num8);
            double num26 = UtilMath.Lerp(bodyTemperature, effectiveFaceTemp, 0.2 + vessel.altitude / vessel.mainBody.Radius * 0.5);
            num26 *= num26;
            num26 *= num26;
            double num27 = 12.566370614359172 * vessel.mainBody.Radius * vessel.mainBody.Radius / (12.566370614359172 * (vessel.mainBody.Radius + vessel.altitude) * (vessel.mainBody.Radius + vessel.altitude));
            bodyEmissiveFlux = PhysicsGlobals.StefanBoltzmanConstant * vessel.mainBody.emissivity * num26 * num27;
            bodyAlbedoFlux = bodySunFlux * 0.5 * (sunDot + 1.0) * vessel.mainBody.albedo * num27;
            bodyEmissiveFlux = UtilMath.Lerp(0.0, bodyEmissiveFlux, t);
            bodyAlbedoFlux = UtilMath.Lerp(0.0, bodyAlbedoFlux, t);
            int num28 = vessel.Parts.Count;
            while (num28-- > 0)
            {
                convectiveTotal += vessel.Parts[num28].thermalConvectionFlux * dTime;
            }
        }
    }
}
