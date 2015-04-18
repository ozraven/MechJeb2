using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace MuMech
{
    public class SimulatedVessel
    {

        private List<SimulatedPart> parts = new List<SimulatedPart>();
        private int count;
        public double totalMass = 0;

        static public SimulatedVessel New(Vessel v)
        {
            SimulatedVessel vessel = new SimulatedVessel();
            vessel.Set(v);
            return vessel;
        }

        private void Set(Vessel v)
        {
            totalMass = 0;


            var oPart = v.Parts;
            count = oPart.Count;

            if (parts.Capacity < count)
                parts.Capacity = count;

            for (int i=0; i < count; i++)
            {
                SimulatedPart simulatedPart = SimulatedPart.New(oPart[i]);
                parts.Add(simulatedPart);
                totalMass += simulatedPart.totalMass;
            }
        }

        public Vector3 Drag(Vector3 velocity, float dynamicPressurekPa, float mach)
        {
            Vector3 drag = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                SimulatedPart part = parts[i];
                //MechJebCore.print(i);
                drag += part.Drag(velocity, dynamicPressurekPa, mach);
            }
            return drag;
        }
    }
}
