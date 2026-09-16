extern alias Firstpass;

using System;
using System.Collections.Generic;
using ArchdruidsAdditions.Data;
using HUD;

namespace ArchdruidsAdditions.Objects.Physical_Objects
{
    public class CollisionBox : IAccessibilityModifier
    {
        public UpdatableAndDeletable owner;
        public CollisionBoxHandler boxHandler;
        public Vector2[] vertices = [];
        public Vector2 cornerPos1, cornerPos2;

        public Vector2 anchorPos;
        public Vector2 lastAnchorPos;

        public AItile.Accessibility[,] baseRoomAccessibilities;

        public CollisionBox(UpdatableAndDeletable owner, Vector2[] vertices)
        {
            if (vertices.Length < 3)
            {
                Debug.Log("   ERROR: COLLISION BOX MUST HAVE 3 OR MORE VERTICES.");
                return;
            }

            this.owner = owner;
            this.vertices = vertices;

            float cornerPos1X = 0;
            float cornerPos1Y = 0;
            float cornerPos2X = 0;
            float cornerPos2Y = 0;

            foreach (Vector2 vert in vertices)
            {
                if (vert.x < cornerPos1X)
                { cornerPos1X = vert.x; }
                if (vert.y < cornerPos1Y)
                { cornerPos1Y = vert.y; }

                if (vert.x > cornerPos2X)
                { cornerPos2X = vert.x; }
                if (vert.y > cornerPos2Y)
                { cornerPos2Y = vert.y; }
            }

            cornerPos1 = new Vector2(cornerPos1X, cornerPos1Y);
            cornerPos2 = new Vector2(cornerPos2X, cornerPos2Y);

            if (!MiscData.boxHandlers.ContainsKey(owner.room))
            {
                CollisionBoxHandler handler = new(owner.room);
                handler.collisionBoxes.Add(this);
            }
            else
            {
                MiscData.boxHandlers[owner.room].collisionBoxes.Add(this);
            }
            boxHandler = MiscData.boxHandlers[owner.room];
        }

        public void SetVertice(int vertice, Vector2 newPos)
        {
            vertices[vertice] = newPos;
        }

        public void Update(Vector2 newAnchorPos)
        {
            lastAnchorPos = anchorPos;

            if (owner.room != null)
            {
                anchorPos = newAnchorPos;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector2 vertice1 = anchorPos + vertices[i];
                    Vector2 vertice2 = anchorPos + ((i == vertices.Length - 1) ? vertices[0] : vertices[i + 1]);

                    Create_LineBetweenTwoPoints(owner.room, vertice1, vertice2, 1f, Color.yellow, 0);
                }
            }

            if (lastAnchorPos != anchorPos)
            {
                boxHandler.CallUpdate();
            }
        }

        public SharedPhysics.TerrainCollisionData UpdateCollisionData(SharedPhysics.TerrainCollisionData data)
        {
            GetSnapPosAndContact(data.pos, data.rad, out Vector2 newPos, out IntVector2 contactPoint, false);

            if (contactPoint.x != 0 || contactPoint.y != 0)
            {
                data.pos = newPos;
                data.vel *= 0f;
                data.contactPoint = contactPoint;
            }

            return data;
        }

        public void GetSnapPosAndContact(Vector2 testPos, float radius, out Vector2 closestPoint, out IntVector2 contactPoint, bool visualize)
        {
            Vector2 relativeTestPos = testPos - anchorPos;
            contactPoint = new IntVector2(0, 0);

            GetClosestPoint(relativeTestPos, out Vector2 newClosestPoint, out float closestPointDist, out int signOfSide);

            closestPoint = anchorPos + newClosestPoint;

            if (signOfSide > 0 || closestPointDist < radius)
            {
                Vector2 dirVec = Custom.DirVec(relativeTestPos, newClosestPoint) * signOfSide;

                if (dirVec.x > 0.5f || dirVec.x < -0.5f)
                { contactPoint.x = -Math.Sign(dirVec.x); }
                
                if (dirVec.y > 0.5f || dirVec.y < -0.5f)
                { contactPoint.y = -Math.Sign(dirVec.y); }
            }
        }

        public bool Contains(Vector2 pos, float rad, bool visualize)
        {
            Vector2 relativeTestPos = pos - anchorPos;

            GetClosestPoint(relativeTestPos, out Vector2 closestPoint, out float closestPointDist, out int signOfSide);

            return signOfSide > 0 || Custom.DistLess(pos, closestPoint + anchorPos, rad);
        }

        public void GetClosestPoint(Vector2 relativePos, out Vector2 closestPoint, out float closestPointDist, out int signOfSide)
        {
            closestPoint = Vector2.zero;
            closestPointDist = float.MaxValue;
            signOfSide = 0;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vert = vertices[i];
                Vector2 vert2 = (i == vertices.Length - 1) ? vertices[0] : vertices[i + 1];

                Vector2 closestSegmentPoint = Custom.ClosestPointOnLineSegment(vert, vert2, relativePos);
                float pointDist = Custom.Dist(relativePos, closestSegmentPoint);

                if (pointDist < closestPointDist)
                {
                    closestPoint = closestSegmentPoint;
                    closestPointDist = pointDist;

                    float distanceToLine = Custom.DistanceToLine(relativePos, vert, vert2);
                    Vector2 lineDir = Custom.DirVec(vert, vert2);


                    if (lineDir.x != 0 && lineDir.y != 0 && ((relativePos.x > vert.x && relativePos.x > vert2.x) || (relativePos.x < vert.x && relativePos.x < vert2.x)))
                    {
                        signOfSide = 0;
                    }
                    else
                    {
                        signOfSide = Math.Sign(distanceToLine);
                    }
                }
            }
        }

        public bool IsTileAccessible(IntVector2 tile, CreatureTemplate crit)
        {
            return !Contains(owner.room.MiddleOfTile(tile), 0f, false);
        }
    }

    public class CollisionBoxHandler : UpdatableAndDeletable
    {
        public List<CollisionBox> collisionBoxes = [];

        public AItile.Accessibility[,] baseTileAccessibilities;
        public AItile.Accessibility[,] newTileAccessibilities;

        public int[,] baseTerrainProximities;
        public int[,] newTerrainProximities;

        public int updateCollisionCounter = 0;

        public bool callForUpdate;

        public CollisionBoxHandler(Room room)
        {
            this.room = room;
            MiscData.boxHandlers.Add(room, this);
            room.AddObject(this);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            float section = 0;

            try
            {

                if (room.readyForAI)
                {
                    section = 1;

                    if (baseTileAccessibilities == null)
                    {
                        baseTileAccessibilities = new AItile.Accessibility[room.Width, room.Height];
                        baseTerrainProximities = new int[room.Width, room.Height];

                        section = 1.2f;

                        for (int i = 0; i < room.Width; i++)
                        {
                            for (int j = 0; j < room.Height; j++)
                            {
                                AItile tile = room.aimap.map[i, j];
                                baseTileAccessibilities[i, j] = tile.acc;

                                baseTerrainProximities[i, j] = room.aimap.terrainProximity[Firstpass.ExtraExtentions.ind(i, j, room.aimap.height)];
                            }
                        }

                        section = 1.3f;

                        newTileAccessibilities = new AItile.Accessibility[room.Width, room.Height];
                        newTerrainProximities = new int[room.Width, room.Height];
                    }

                    section = 2;

                    if (updateCollisionCounter > 0)
                    {
                        updateCollisionCounter--;
                    }
                    else if (callForUpdate && newTileAccessibilities != null)
                    {
                        callForUpdate = false;
                        updateCollisionCounter = 5;

                        for (int i = 0; i < room.Width; i++)
                        {
                            for (int j = 0; j < room.Height; j++)
                            {
                                AItile.Accessibility newAcc = GetTrueAccessibilityOfTile(new IntVector2(i, j), baseTileAccessibilities[i, j]);
                                room.aimap.map[i, j].acc = newAcc;
                                newTileAccessibilities[i, j] = newAcc;

                                if (baseTerrainProximities != null)
                                {
                                    int terrainProximity = Mathf.Min(baseTerrainProximities[i, j], GetTerrainProximity(i, j));
                                    room.aimap.terrainProximity[Firstpass.ExtraExtentions.ind(i, j, room.aimap.height)] = terrainProximity;
                                    newTerrainProximities[i, j] = terrainProximity;
                                }
                            }
                        }

                        section = 2.2f;
                    }
                }

            }
            catch (Exception e)
            {
                Log_Exception(e, "COLLISIONBOXHANDLER_UPDATE", section);
            }
        }

        public void CallUpdate()
        {
            callForUpdate = true;
        }

        public bool TrySnapToCollisionBox(Vector2 testPos, float radius, out Vector2 snapPos, out Vector2 surfaceDir, out IntVector2 contactPoint)
        {
            snapPos = testPos;
            contactPoint = new(0, 0);
            surfaceDir = Vector2.zero;

            Vector2 testPos1 = new(testPos.x, testPos.y - radius);
            Vector2 testPos2 = new(testPos.x, testPos.y + radius);

            bool pushOutOfBox1 = false;

            Vector2 bestSnapPos = testPos;
            Vector2 bestSurfaceDir = Vector2.zero;
            float snapDist = float.MaxValue;
            foreach (CollisionBox box in collisionBoxes)
            {
                Vector2 rTestPos = testPos - box.anchorPos;
                Vector2 rTestPos1 = testPos1 - box.anchorPos;
                Vector2 rTestPos2 = testPos2 - box.anchorPos;

                bool pushOutOfBox2 = false;

                for (int i = 0; i < box.vertices.Length; i++)
                {
                    Vector2 vertice1 = box.vertices[i];
                    Vector2 vertice2 = i == box.vertices.Length - 1 ? box.vertices[0] : box.vertices[i + 1];

                    if ((vertice1.x <= rTestPos.x && vertice2.x > rTestPos.x) || (vertice1.x > rTestPos.x && vertice2.x <= rTestPos.x))
                    {
                        float lerpX = Mathf.InverseLerp(vertice1.x, vertice2.x, rTestPos.x);
                        Vector2 testSnapPos = Vector2.Lerp(vertice1, vertice2, lerpX);
                        float testSnapDist = Custom.DistNoSqrt(rTestPos, testSnapPos);

                        if (testSnapDist < snapDist && Custom.InRange(testSnapPos.y, rTestPos1.y - 5f, rTestPos2.y + 5f))
                        {
                            bestSnapPos = testSnapPos + box.anchorPos;

                            Vector2 segmentDir = Custom.DirVec(vertice1, vertice2);
                            bestSurfaceDir = -Custom.PerpendicularVector(Custom.DirVec(vertice1, vertice2));
                            snapDist = testSnapDist;
                            pushOutOfBox2 = true;
                        }
                    }
                }

                if (pushOutOfBox2)
                {
                    if (box.Contains(testPos1, 0f, false))
                    {
                        pushOutOfBox1 = true;
                    }
                }
            }

            if (snapDist < float.MaxValue)
            {
                snapPos = bestSnapPos;
                surfaceDir = bestSurfaceDir;

                if (snapPos.y > testPos1.y && pushOutOfBox1)
                { contactPoint.y = -1; }
                else if (snapPos.y < testPos2.y && pushOutOfBox1)
                { contactPoint.y = 1; }

                if (snapPos.x > testPos.x)
                { contactPoint.x = -1; }
                else if (snapPos.x < testPos.x)
                { contactPoint.x = 1; }

                return pushOutOfBox1;
            }

            return false;
        }

        public bool PositionInsideBox(Vector2 pos, float rad, bool visualize)
        {
            foreach (CollisionBox box in collisionBoxes)
            {
                if (box.Contains(pos, rad, visualize))
                { return true; }
            }
            return false;
        }

        public AItile.Accessibility GetTrueAccessibilityOfTile(IntVector2 pos, AItile.Accessibility baseAcc)
        {
            if (baseAcc == AItile.Accessibility.OffScreen || baseAcc == AItile.Accessibility.Solid || baseAcc == AItile.Accessibility.Sand)
            { return baseAcc; }

            AItile.Accessibility bestAccessibility = baseAcc;
            foreach (CollisionBox box in collisionBoxes)
            {
                Vector2 relativeTestPos = room.MiddleOfTile(pos) - box.anchorPos;

                Vector2 closestPoint = Vector2.zero;
                float closestPointDist = float.MaxValue;
                int signOfSide = 0;

                for (int i = 0; i < box.vertices.Length; i++)
                {
                    Vector2 vert = box.vertices[i];
                    Vector2 vert2 = (i == box.vertices.Length - 1) ? box.vertices[0] : box.vertices[i + 1];

                    Vector2 closestSegmentPoint = Custom.ClosestPointOnLineSegment(vert, vert2, relativeTestPos);
                    float pointDist = Custom.Dist(relativeTestPos, closestSegmentPoint);

                    if (pointDist < closestPointDist)
                    {
                        closestPoint = closestSegmentPoint;
                        closestPointDist = pointDist;

                        signOfSide = Math.Sign(Custom.DistanceToLine(relativeTestPos, vert, vert2));
                    }
                }

                if (closestPointDist < 20)
                {
                    if (signOfSide > 0)
                    {
                        return AItile.Accessibility.Solid;
                    }
                    else if (bestAccessibility != AItile.Accessibility.Corridor && bestAccessibility != AItile.Accessibility.Climb)
                    {
                        Vector2 dirvec = Custom.DirVec(relativeTestPos, closestPoint);

                        if (dirvec.y < -0.1f)
                        {
                            Create_Square(room, relativeTestPos + box.anchorPos, 10f, 10f, Vec(0), Color.blue, 10);
                            Create_LineBetweenTwoPoints(room, relativeTestPos + box.anchorPos, closestPoint + box.anchorPos, 2f, Color.blue, 10);

                            if (bestAccessibility == AItile.Accessibility.Ceiling)
                            { bestAccessibility = AItile.Accessibility.Corridor; }
                            else
                            { bestAccessibility = AItile.Accessibility.Floor; }
                        }
                        else if (dirvec.x > 0.5f || dirvec.x < -0.5f)
                        {
                            Create_Square(room, relativeTestPos + box.anchorPos, 10f, 10f, Vec(0), Color.red, 10);
                            Create_LineBetweenTwoPoints(room, relativeTestPos + box.anchorPos, closestPoint + box.anchorPos, 2f, Color.red, 10);

                            bestAccessibility = AItile.Accessibility.Wall;
                        }
                        else
                        {
                            Create_Square(room, relativeTestPos + box.anchorPos, 10f, 10f, Vec(0), Color.green, 10);
                            Create_LineBetweenTwoPoints(room, relativeTestPos + box.anchorPos, closestPoint + box.anchorPos, 2f, Color.green, 10);

                            if (bestAccessibility == AItile.Accessibility.Floor)
                            { bestAccessibility = AItile.Accessibility.Corridor; }
                            else
                            { bestAccessibility = AItile.Accessibility.Ceiling; }
                        }
                    }
                }
            }

            return bestAccessibility;
        }

        public int GetTerrainProximity(int x, int y)
        {
            Vector2 pos = room.MiddleOfTile(x, y);

            Vector2 nearestBoxPos = new(-10000, -10000);
            float nearestBoxDist = float.MaxValue;
            foreach (CollisionBox box in collisionBoxes)
            {
                box.GetSnapPosAndContact(pos, 0f, out Vector2 closestPoint, out _, false);
                float dist = Custom.Dist(pos, closestPoint);

                if (dist < nearestBoxDist)
                {
                    nearestBoxPos = closestPoint;
                    nearestBoxDist = dist;
                }
            }

            if (nearestBoxDist < float.MaxValue)
            { return Custom.ManhattanDistance(new IntVector2(x, y), room.GetTilePosition(nearestBoxPos)); }

            return int.MaxValue;
        }
    }
}
