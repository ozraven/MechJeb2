﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace MuMech
{
    public class SimulatedVessel
    {

        public List<SimulatedPart> parts = new List<SimulatedPart>();
        private int count;
        public double totalMass = 0;

        private ReentrySimulation.SimCurves simCurves;

        static public SimulatedVessel New(Vessel v, ReentrySimulation.SimCurves simCurves)
        {
            SimulatedVessel vessel = new SimulatedVessel();
            vessel.Set(v, simCurves);
            return vessel;
        }


        private void Set(Vessel v, ReentrySimulation.SimCurves _simCurves)
        {
            totalMass = 0;

            var oParts = v.Parts;
            count = oParts.Count;

            simCurves = _simCurves;

            if (parts.Capacity < count)
                parts.Capacity = count;

            for (int i=0; i < count; i++)
            {
                SimulatedPart simulatedPart = SimulatedPart.New(oParts[i], simCurves);
                parts.Add(simulatedPart);
                totalMass += simulatedPart.totalMass;
            }
        }

        public Vector3 Drag(Vector3 velocity, Quaternion attitude, float dynamicPressurekPa, float mach)
        {
            Vector3 drag = Vector3.zero;

            Vector3 vesselLocalVel = attitude * Vector3.up * velocity.magnitude;

            for (int i = 0; i < count; i++)
            {
                SimulatedPart part = parts[i];
                //MechJebCore.print(i);
                drag += part.Drag(vesselLocalVel, dynamicPressurekPa, mach);
            }

            return -velocity.normalized * drag.magnitude;
        }



        public Vector3 Lift(Vector3 velocity, Quaternion attitude, float dynamicPressurekPa, float mach)
        {
            Vector3 lift = Vector3.zero;

            //return lift;

            for (int i = 0; i < count; i++)
            {
                SimulatedPart part = parts[i];
                //MechJebCore.print(i);
                lift += part.Lift(velocity, dynamicPressurekPa, mach);
            }
            return lift;
        }
    }
}
