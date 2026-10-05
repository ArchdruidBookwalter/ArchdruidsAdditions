using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using Steamworks;

using UnityEngineMatrix = UnityEngine.Matrix4x4;

namespace ArchdruidsAdditions.Hooks;

public static class LizardHooks
{
    internal static List<string> ILstrings = [];
    //internal static bool printString = true;

    //internal static bool ILHookSuccessful1 = false;
    //internal static bool ILHookSuccessful2 = false;
    //internal static int hooks = 0;

    internal static void Lizard_Act(On.Lizard.orig_Act orig, Lizard self)
    {
        orig(self);

        /*
        if (printString)
        {
            Debug.Log("");
            Debug.Log("HOOKS: " + hooks);
            foreach (string ilString in ILstrings)
            {
                Debug.Log(ilString);
            }
            printString = false;
        }*/
    }

    internal static void IL_Lizard_FollowConnection(ILContext context)
    {
        ILCursor cursor = new(context);

        int variableIndex = -1;
        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld(typeof(UpdatableAndDeletable).GetField(nameof(UpdatableAndDeletable.room))),
            x => x.MatchLdarg(0),
            x => x.MatchLdflda(typeof(Lizard).GetField(nameof(Lizard.followingConnection))),
            x => x.MatchCallOrCallvirt(typeof(MovementConnection), "get_DestTile"),
            x => x.MatchCallOrCallvirt(typeof(Room), nameof(Room.MiddleOfTile)),
            x => x.MatchStloc(out variableIndex)))
        {
            //ILstrings.Add("BEFORE HOOK IL STRUCTURE 1: ");
            //LogInstructions(context, cursor);

            ILLabel oldExitLabel = null;

            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchBrfalse(out oldExitLabel)))
            {
                cursor.GotoLabel(oldExitLabel);

                ILLabel newExitLabel = cursor.DefineLabel();

                ILCursor newCursor = new(cursor);
                newCursor.Emit(OpCodes.Br, newExitLabel);

                VariableDefinition newVar = new(context.Import(typeof(Vector2)));
                cursor.Body.Variables.Add(newVar);

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, variableIndex);
                cursor.Emit(OpCodes.Ldloca, newVar);
                cursor.EmitDelegate(IsPosOnBoxDelegate);
                cursor.Emit(OpCodes.Brfalse, newExitLabel);
                cursor.Emit(OpCodes.Ldloc, newVar);
                cursor.Emit(OpCodes.Stloc, newVar);
                cursor.MarkLabel(newExitLabel);

                //hooks++;
                //ILstrings.Add("AFTER HOOK IL STRUCTURE 1: ");
                //LogInstructions(context, cursor);
            }
        }

        int variableIndex2 = -1;
        if (cursor.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt(typeof(MovementConnection), "get_DestTile"),
            x => x.MatchCallOrCallvirt(typeof(Room), nameof(Room.MiddleOfTile)),
            x => x.MatchStloc(out variableIndex2)))
        {
            //ILstrings.Add("BEFORE HOOK IL STRUCTURE 2: ");
            //LogInstructions(context, cursor);

            ILLabel oldExitLabel = null;

            if (cursor.TryGotoNext(MoveType.After,
                x => x.MatchBrfalse(out oldExitLabel)))
            {
                cursor.GotoLabel(oldExitLabel);

                ILLabel newExitLabel = cursor.DefineLabel();

                ILCursor newCursor = new(cursor);
                newCursor.Emit(OpCodes.Br, newExitLabel);

                VariableDefinition newVar = new(context.Import(typeof(Vector2)));
                cursor.Body.Variables.Add(newVar);

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, variableIndex);
                cursor.Emit(OpCodes.Ldloca, newVar);
                cursor.EmitDelegate(IsPosOnBoxDelegate);
                cursor.Emit(OpCodes.Brfalse, newExitLabel);
                cursor.Emit(OpCodes.Ldloc, newVar);
                cursor.Emit(OpCodes.Stloc, newVar);
                cursor.MarkLabel(newExitLabel);

                //hooks++;
                //ILstrings.Add("AFTER HOOK IL STRUCTURE 2: ");
                //LogInstructions(context, cursor);
            }
        }
    }
    internal static bool IsPosOnBoxDelegate(Lizard self, Vector2 pos, out Vector2 newPos)
    {
        Create_Text(self.room, self.mainBodyChunk.pos + new Vector2(0f, 40f), "DELEGATE 1 WAS CALLED!", Color.red, 0);
        
        newPos = pos;

        if (MiscData.boxHandlers.ContainsKey(self.room) && MiscData.boxHandlers[self.room].TryGetSurfacePos(pos, 10f, out Vector2 surfacePos, out _, out float surfacePosDist))
        {
            if (surfacePosDist < 30f)
            {
                newPos = surfacePos;
                return true; 
            }
        }
        return false;
    }
    public static void LogInstructions(ILContext context, ILCursor cursor)
    {
        for (int i = -30; i < 0; i++)
        {
            Instruction instr = context.Instrs[cursor.Index + i];
            string ILstring = "        ";
            try
            {
                ILstring += instr.ToString();
            }
            catch
            {
                if (instr.MatchBrfalse(out _))
                { ILstring += "BRFALSE"; }
                else if (instr.MatchBneUn(out _))
                { ILstring += "BNEUN"; }
                else if (instr.MatchBle(out _))
                { ILstring += "BLE"; }
                else if (instr.MatchBr(out _))
                { ILstring += "BR"; }
            }
            ILstrings.Add(ILstring);
        }

        Instruction instr2 = context.Instrs[cursor.Index];
        string ILstring2 = "      C>";
        try
        {
            ILstring2 += instr2.ToString();
        }
        catch
        {
            if (instr2.MatchBrfalse(out _))
            { ILstring2 += "BRFALSE"; }
            else if (instr2.MatchBneUn(out _))
            { ILstring2 += "BNEUN"; }
            else if (instr2.MatchBle(out _))
            { ILstring2 += "BLE"; }
            else if (instr2.MatchBr(out _))
            { ILstring2 += "BR"; }
        }
        ILstrings.Add(ILstring2);

        for (int i = 1; i < 30; i++)
        {
            Instruction instr = context.Instrs[cursor.Index + i];
            string ILstring = "        ";
            try
            {
                ILstring += instr.ToString();
            }
            catch
            {
                if (instr.MatchBrfalse(out _))
                { ILstring += "BRFALSE"; }
                else if (instr.MatchBneUn(out _))
                { ILstring += "BNEUN"; }
                else if (instr.MatchBle(out _))
                { ILstring += "BLE"; }
                else if (instr.MatchBr(out _))
                { ILstring += "BR"; }
            }
            ILstrings.Add(ILstring);
        }
    }

    internal static void LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
    {
        orig(self);
    }

    internal static void LizardGraphics_Update(On.LizardGraphics.orig_Update orig, LizardGraphics self)
    {
        orig(self);
    }

    internal static void LizardLimb_Update(On.LizardLimb.orig_Update orig, LizardLimb self)
    {
        orig(self);

        if (self.owner.owner.room != null && self.mode == Limb.Mode.HuntAbsolutePosition)
        {
            /*
            Create_LineBetweenTwoPoints(self.owner.owner.room, self.pos, self.absoluteHuntPos, 1f, self.reachingForTerrain ? Color.red : Color.green, 0);
            Create_Square(self.owner.owner.room, self.absoluteHuntPos, 2f, 2f, Vec(45), self.reachingForTerrain ? Color.red : Color.green, 0);*/
        }
    }
}
