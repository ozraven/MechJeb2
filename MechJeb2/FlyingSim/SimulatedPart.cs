using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuMech
{
    class SimulatedPart
    {

        private DragCubeList cubes = new DragCubeList();
        
        public double totalMass = 0;
        public bool shieldedFromAirstream;
        public bool noDrag;
        public bool hasLiftModule;
        private float bodyLiftMultiplier;

        // Remove after test
        private Part oPart;


        private Quaternion vesselToPart;
        
        static public SimulatedPart New(Part p)
        {
            SimulatedPart part = new SimulatedPart();
            part.Set(p);
            return part;
        }

        private void Set(Part p)
        {
            totalMass = p.mass + p.GetResourceMass() + p.GetPhysicslessChildMass();
            shieldedFromAirstream = p.ShieldedFromAirstream;

            noDrag = p.rb == null && !PhysicsGlobals.ApplyDragToNonPhysicsParts;
            hasLiftModule = p.hasLiftModule;
            bodyLiftMultiplier = p.bodyLiftMultiplier;

            CopyDragCubesList(p.DragCubes, cubes);

            // Rotation to convert the vessel space velocity to the part space velocity
            vesselToPart = Quaternion.LookRotation(p.vessel.GetTransform().InverseTransformDirection(p.transform.forward), p.vessel.GetTransform().InverseTransformDirection(p.transform.up)).Inverse();

            if (p.dragModel != Part.DragModel.CUBE)
                MechJebCore.print(p.name + " " + p.dragModel);

            oPart = p;

        }

        public Vector3 Drag(Vector3 velocity, float dynamicPressurekPa,  float mach)
        {
            if (shieldedFromAirstream || noDrag)
                return Vector3.zero;

            Vector3 dragDir = -(vesselToPart * velocity).normalized;

            cubes.SetDrag(dragDir, mach);

            //MechJebCore.print(velocity.normalized.magnitude.ToString("F3") + " " + cubes.AreaDrag.ToString("F3") + " " + dynamicPressurekPa.ToString("F7") + " " + PhysicsGlobals.DragCubeMultiplier.ToString("F3") + " " + PhysicsGlobals.DragMultiplier.ToString("F3"));

#warning do some of this math once per frame
            Vector3 drag = -velocity.normalized * cubes.AreaDrag * dynamicPressurekPa * PhysicsGlobals.DragCubeMultiplier * PhysicsGlobals.DragMultiplier;


            //bool delta = false;
            //string msg = oPart.name;

            //if (velocity.sqrMagnitude > 1 && dynamicPressurekPa - oPart.dynamicPressurekPa > oPart.dynamicPressurekPa * 0.1)
            //{
            //    msg += " dynamicPressurekPa " + dynamicPressurekPa.ToString("f4") + " vs " + oPart.dynamicPressurekPa.ToString("f4");
            //    delta = true;
            //}
            //
            ////if (velocity.sqrMagnitude > 1 && cubes.AreaDrag - oPart.DragCubes.AreaDrag > oPart.DragCubes.AreaDrag * 0.1)
            //if (velocity.sqrMagnitude > 1)
            //{
            //    msg += "\n AreaDrag " + cubes.AreaDrag.ToString("f4") + " vs " + oPart.DragCubes.AreaDrag.ToString("f4");
            //    //msg += "\n mach "     + mach.ToString("f4")           + " vs " + oPart.machNumber.ToString("f4");
            //    
            //    msg += "\n dragDir " + MuUtils.PrettyPrint(dragDir)             + " vs " + MuUtils.PrettyPrint(oPart.dragVectorDirLocal)    + " " + Vector3.Angle(dragDir, oPart.dragVectorDirLocal).ToString("F3") + "°";
            //    //msg += "\n dragVel " + MuUtils.PrettyPrint(velocity.normalized) + " vs " + MuUtils.PrettyPrint(oPart.dragVector.normalized) + " " + Vector3.Angle(velocity.normalized, oPart.dragVector).ToString("F3") + "°";
            //    
            //    msg += "\n Real° " + MuUtils.PrettyPrint(oPart.dragVectorDirLocal) + " " + Vector3.Angle(oPart.dragVectorDirLocal, Vector3.down).ToString("F3") + "°";
            //    msg += "\n Sim°  " + MuUtils.PrettyPrint(dragDir)                  + " " + Vector3.Angle(dragDir, Vector3.down).ToString("F3") + "°";
            //
            //    msg += "\n toUp " + MuUtils.PrettyPrint(vesselToPart * Vector3.up) + Vector3.Angle(vesselToPart * Vector3.up, Vector3.up).ToString("F3") + "°";
            //
            //
            //    Vector3 quatUp = vesselToPart * Vector3.up;
            //    Vector3 shipUp = oPart.vessel.transform.InverseTransformDirection(oPart.transform.up);
            //
            //    msg += "\n Ups " + MuUtils.PrettyPrint(quatUp) + " vs " + MuUtils.PrettyPrint(shipUp) + " " + Vector3.Angle(quatUp, shipUp).ToString("F3") + "°";
            //
            //
            //    
            //    //msg += "\n AreaOccluded ";
            //    //for (int i = 0; i < 6; i++)
            //    //{
            //    //    msg += cubes.AreaOccluded[i].ToString("F3") + "/" + oPart.DragCubes.AreaOccluded[i].ToString("F3") + " ";
            //    //}
            //    //msg += "\n WeightedDrag ";
            //    //for (int i = 0; i < 6; i++)
            //    //{
            //    //    msg += cubes.WeightedDrag[i].ToString("F3") + "/" + oPart.DragCubes.WeightedDrag[i].ToString("F3") + " ";
            //    //}
            //
            //    msg += "\n vesselToPart " + MuUtils.PrettyPrint(vesselToPart.eulerAngles);
            //    delta = true;
            //}
            //
            //if (delta)
            //    MechJebCore.print(msg);


            //MechJebCore.print(MuUtils.PrettyPrint(velocity.normalized) + " " + MuUtils.PrettyPrint(dragDir) + " " + MuUtils.PrettyPrint(drag));

            return drag;
        }

        public Vector3 Lift(Vector3 velocity, float dynamicPressurekPa, float mach)
        {
            if (hasLiftModule)
                return Vector3.zero;

#warning obviously move out of here and evaluate once per mach value
            var liftCurves = PhysicsGlobals.GetLiftingSurfaceCurve("BodyLift");

            float lift = bodyLiftMultiplier * dynamicPressurekPa * PhysicsGlobals.BodyLiftMultiplier * liftCurves.liftMachCurve.Evaluate(mach);
            Vector3 liftV = vesselToPart.Inverse() * cubes.LiftForce * lift;
            return Vector3.ProjectOnPlane(liftV, velocity);
        }


        public static void CopyDragCubesList(DragCubeList source, DragCubeList dest)
        {
            dest.ClearCubes();

            dest.None = source.None;
            dest.Procedural = source.Procedural;

            for (int i = 0; i < source.Cubes.Count; i++)
            {
                DragCube c = new DragCube();
                CopyDragCube(source.Cubes[i], c);
                dest.Cubes.Add(c);
            }
            
            dest.SetDragWeights();

            for (int i=0; i<6; i++)
            {
                dest.WeightedArea[i] = source.WeightedArea[i];
                dest.WeightedDrag[i] = source.WeightedDrag[i];
                dest.AreaOccluded[i] = source.AreaOccluded[i];
                dest.DragModifiers[i] = source.DragModifiers[i];
            }
            // We are missing PostOcclusionArea but it seems to be used in Thermal only
        }

        private static void CopyDragCube(DragCube source, DragCube dest)
        {
            dest.Name = source.Name;
            dest.Weight = source.Weight;
            for (int i = 0; i < source.Drag.Length; i++)
            {
                dest.Drag[i] = source.Drag[i];
                dest.Area[i] = source.Area[i];
                dest.DragModifiers[i] = source.DragModifiers[i];
            }
        }


    }
}
