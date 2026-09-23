extern alias Firstpass;

using System;
using System.Collections.Generic;

namespace ArchdruidsAdditions.Objects
{
    public class CollisionBox : IAccessibilityModifier
    {
        public UpdatableAndDeletable owner;
        public CollisionBoxHandler boxHandler;
        public Vector2[] vertices = [];
        public Vector2[] baseVertices = [];
        public Vector2 cornerPos1, cornerPos2;

        public Vector2 anchorPos;
        public Vector2 lastAnchorPos;
        public Vector2 vel;

        public AItile.Accessibility[,] baseRoomAccessibilities;

        public float rotation;

        public List<PhysicalObject> objectsStandingOnMe;

        public CollisionBox(UpdatableAndDeletable owner, Vector2[] vertices)
        {
            if (vertices.Length < 3)
            {
                Debug.Log("   ERROR: COLLISION BOX MUST HAVE 3 OR MORE VERTICES.");
                return;
            }

            this.owner = owner;
            this.vertices = vertices;

            baseVertices = new Vector2[vertices.Length];
            vertices.CopyTo(baseVertices, 0);

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
            baseVertices[vertice] = newPos;
        }

        public void Update(Vector2 newAnchorPos, Vector2 newVel, float newRotation)
        {
            lastAnchorPos = anchorPos;

            if (owner.room != null)
            {
                anchorPos = newAnchorPos;
                rotation = newRotation;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = Custom.rotateVectorDeg(baseVertices[i], newRotation);

                    Vector2 vertice1 = anchorPos + vertices[i];
                    Vector2 vertice2 = anchorPos + (i == vertices.Length - 1 ? vertices[0] : vertices[i + 1]);

                    Create_LineBetweenTwoPoints(owner.room, vertice1, vertice2, 1f, Color.yellow, 0);
                }
            }

            if (lastAnchorPos != anchorPos)
            {
                boxHandler.CallUpdate();
            }

            vel = newVel;

            /*
            Vector2 dir = Custom.DirVec(lastAnchorPos, anchorPos) * Custom.Dist(lastAnchorPos, anchorPos);
            foreach (PhysicalObject obj in objectsStandingOnMe)
            {
                foreach (BodyChunk chunk in obj.bodyChunks)
                {
                    chunk.pos += dir;
                    chunk.vel += dir.normalized;
                }
            }*/
        }

        public SharedPhysics.TerrainCollisionData UpdateCollisionData(SharedPhysics.TerrainCollisionData data)
        {
            GetSnapPosAndContact(data.pos, data.lastPos, data.rad, out Vector2 newPos, out IntVector2 contactPoint, false);

            if (contactPoint.x != 0 || contactPoint.y != 0)
            {
                data.pos = newPos;
                data.vel *= 0f;
                data.contactPoint = contactPoint;
            }

            return data;
        }

        public void GetSnapPosAndContact(Vector2 pos, Vector2 lastPos, float radius, out Vector2 snapPos, out IntVector2 contactPoint, bool visualize)
        {
            Vector2 relativeTestPos = pos - anchorPos;
            contactPoint = new IntVector2(0, 0);

            Vector2 rTestPos = pos - anchorPos;
            Vector2 rLastPos = lastPos - anchorPos;

            /*
            snapPos = new Vector2(-1, -1);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertice1 = vertices[i];
                Vector2 vertice2 = i == vertices.Length - 1 ? vertices[0] : vertices[i + 1];

                Vector2 intersect = Custom.LineIntersection(vertice1, vertice2, rTestPos, rLastPos);
                if (Custom.InRange(intersect.x, Mathf.Max(vertice1.x, vertice2.x), Mathf.Min(vertice1.x, vertice2.x)) &&
                    Custom.InRange(intersect.y, Mathf.Max(vertice1.y, vertice2.y), Mathf.Min(vertice1.y, vertice2.y)) &&
                    Custom.InRange(intersect.x, Mathf.Max(rTestPos.x, rLastPos.x) + radius, Mathf.Min(rTestPos.x, rLastPos.x) - radius) &&
                    Custom.InRange(intersect.y, Mathf.Max(rTestPos.y, rLastPos.y) + radius, Mathf.Min(rTestPos.y, rLastPos.y) - radius))
                {
                    //Create_Square(owner.room, intersect + anchorPos, 5f, 5f, Vec(45), Color.red, 20);
                    snapPos = intersect + anchorPos;
                }
            }

            if (newSnapPos.x != -1 && newSnapPos.y != -1)
            {
                Vector2 dirVec = Custom.DirVec(pos, newSnapPos);

                if (dirVec.x > 0.5f || dirVec.x < -0.5f)
                { contactPoint.x = -Math.Sign(dirVec.x); }

                if (dirVec.y > 0.5f || dirVec.y < -0.5f)
                { contactPoint.y = -Math.Sign(dirVec.y); }

                snapPos = newSnapPos;
            }*/

            GetClosestPoint(relativeTestPos, out Vector2 newClosestPoint, out float closestPointDist, out int signOfSide);

            snapPos = anchorPos + newClosestPoint;

            if (signOfSide > 0 || closestPointDist < radius)
            {
                Vector2 dirVec = Custom.DirVec(relativeTestPos, newClosestPoint) * signOfSide;

                if (dirVec.x > 0.5f || dirVec.x < -0.5f)
                { contactPoint.x = -Math.Sign(dirVec.x); }

                if (dirVec.y > 0.5f || dirVec.y < -0.5f)
                { contactPoint.y = -Math.Sign(dirVec.y); }
            }
        }

        /*
        public void GetSnapPos(Vector2 pos, Vector2 lastPos, float radius, out Vector2 snapPos)
        {
            Vector2 rTestPos = pos - anchorPos;
            Vector2 rLastPos = lastPos - anchorPos;

            snapPos = new Vector2(-1, -1);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertice1 = vertices[i];
                Vector2 vertice2 = i == vertices.Length - 1 ? vertices[0] : vertices[i + 1];

                Vector2 intersect = Custom.LineIntersection(vertice1, vertice2, rTestPos, rLastPos);
                if (Custom.InRange(intersect.x, Mathf.Max(vertice1.x, vertice2.x), Mathf.Min(vertice1.x, vertice2.x)) &&
                    Custom.InRange(intersect.y, Mathf.Max(vertice1.y, vertice2.y), Mathf.Min(vertice1.y, vertice2.y)) &&
                    Custom.InRange(intersect.x, Mathf.Max(rTestPos.x, rLastPos.x) + radius, Mathf.Min(rTestPos.x, rLastPos.x) - radius) &&
                    Custom.InRange(intersect.y, Mathf.Max(rTestPos.y, rLastPos.y) + radius, Mathf.Min(rTestPos.y, rLastPos.y) - radius))
                {
                    //Create_Square(owner.room, intersect + anchorPos, 5f, 5f, Vec(45), Color.red, 20);
                    snapPos = intersect + anchorPos;
                }
            }
        }*/

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
                Vector2 vert2 = i == vertices.Length - 1 ? vertices[0] : vertices[i + 1];


                Vector2 closestSegmentPoint = Custom.ClosestPointOnLineSegment(vert, vert2, relativePos);
                float pointDist = Custom.Dist(relativePos, closestSegmentPoint);

                if (pointDist < closestPointDist)
                {
                    closestPoint = closestSegmentPoint;
                    closestPointDist = pointDist;

                    float distanceToLine = Custom.DistanceToLine(relativePos, vert, vert2);
                    Vector2 lineDir = Custom.DirVec(vert, vert2);


                    if (lineDir.x != 0 && lineDir.y != 0 && (relativePos.x > vert.x && relativePos.x > vert2.x || relativePos.x < vert.x && relativePos.x < vert2.x))
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
        public IntVector2 updateTileCoord;

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
                        updateCollisionCounter = 1;

                        for (int i = 0; i < 50; i++)
                        {
                            updateTileCoord.x += 1;

                            if (updateTileCoord.x >= room.Width - 1)
                            {
                                if (updateTileCoord.y >= room.Height - 1)
                                { updateTileCoord.y = 0; }
                                else
                                { updateTileCoord.y += 1; }

                                updateTileCoord.x = 0;
                            }

                            AItile.Accessibility newAcc = GetTrueAccessibilityOfTile(updateTileCoord, baseTileAccessibilities[updateTileCoord.x, updateTileCoord.y]);
                            room.aimap.map[updateTileCoord.x, updateTileCoord.y].acc = newAcc;
                            newTileAccessibilities[updateTileCoord.x, updateTileCoord.y] = newAcc;

                            if (baseTerrainProximities != null)
                            {
                                int terrainProximity = Mathf.Min(baseTerrainProximities[updateTileCoord.x, updateTileCoord.y], GetTerrainProximity(updateTileCoord.x, updateTileCoord.y));
                                room.aimap.terrainProximity[Firstpass.ExtraExtentions.ind(updateTileCoord.x, updateTileCoord.y, room.aimap.height)] = terrainProximity;
                                newTerrainProximities[updateTileCoord.x, updateTileCoord.y] = terrainProximity;
                            }

                            //Create_Square(room, room.MiddleOfTile(updateTileCoord), 10f, 10f, Vec(45), "Red", 5);
                        }

                        section = 2.1f;
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

        public bool TrySnapToCollisionBox(Vector2 pos, Vector2 lastPos, float radius, out Vector2 snapPos, out Vector2 surfaceDir, out IntVector2 contactPoint, out CollisionBox outBox, BodyChunk chunk = null)
        {
            snapPos = pos;
            contactPoint = new(0, 0);
            surfaceDir = Vector2.zero;
            outBox = null;

            Vector2 testPos1 = new(pos.x, pos.y - radius);
            Vector2 testPos2 = new(pos.x, pos.y + radius);
            Vector2 lastPos1 = new(lastPos.x, lastPos.y - radius);
            Vector2 lastPos2 = new(lastPos.x, lastPos.y + radius);

            bool pushOutOfBox = false;
            bool pushToLastPos = false;

            Vector2 bestSnapPos1 = pos;
            Vector2 bestSurfaceDir1 = Vector2.zero;
            float snapDist1 = float.MaxValue;

            Vector2 bestSnapPos2 = lastPos;
            Vector2 bestSurfaceDir2 = Vector2.zero;
            float snapDist2 = float.MaxValue;
            foreach (CollisionBox box in collisionBoxes)
            {
                if (chunk == null || chunk.owner != box.owner)
                {
                    Vector2 rTestPos = pos - box.anchorPos;
                    Vector2 rLastPos = lastPos - box.anchorPos;

                    Vector2 rTestPos1 = testPos1 - box.anchorPos;
                    Vector2 rTestPos2 = testPos2 - box.anchorPos;

                    bool pushOutOfBox1 = false;
                    bool pushOutOfBox2 = false;

                    for (int i = 0; i < box.vertices.Length; i++)
                    {
                        Vector2 vertice1 = box.vertices[i];
                        Vector2 vertice2 = i == box.vertices.Length - 1 ? box.vertices[0] : box.vertices[i + 1];

                        if (vertice1.x <= rTestPos.x && vertice2.x > rTestPos.x || vertice1.x > rTestPos.x && vertice2.x <= rTestPos.x)
                        {
                            float lerpX = Mathf.InverseLerp(vertice1.x, vertice2.x, rTestPos.x);
                            Vector2 testSnapPos = Vector2.Lerp(vertice1, vertice2, lerpX);
                            float testSnapDist = Custom.DistNoSqrt(rTestPos, testSnapPos);

                            if (testSnapDist < snapDist1 && Custom.InRange(testSnapPos.y, rTestPos1.y - 5f, rTestPos2.y + 5f))
                            {
                                bestSnapPos1 = testSnapPos + box.anchorPos;

                                Vector2 segmentDir = Custom.DirVec(vertice1, vertice2);
                                bestSurfaceDir1 = -Custom.PerpendicularVector(Custom.DirVec(vertice1, vertice2));
                                snapDist1 = testSnapDist;
                                pushOutOfBox1 = true;
                            }
                        }

                        if (vertice1.x <= rLastPos.x && vertice2.x > rLastPos.x || vertice1.x > rLastPos.x && vertice2.x <= rLastPos.x)
                        {
                            float lerpX = Mathf.InverseLerp(vertice1.x, vertice2.x, rLastPos.x);
                            Vector2 testSnapPos = Vector2.Lerp(vertice1, vertice2, lerpX);
                            float testSnapDist = Custom.DistNoSqrt(rLastPos, testSnapPos);

                            if (testSnapDist < snapDist2 && Custom.InRange(testSnapPos.y, rLastPos.y - 5f, rLastPos.y + 5f))
                            {
                                bestSnapPos2 = testSnapPos + box.anchorPos;

                                Vector2 segmentDir = Custom.DirVec(vertice1, vertice2);
                                bestSurfaceDir2 = -Custom.PerpendicularVector(Custom.DirVec(vertice1, vertice2));
                                snapDist2 = testSnapDist;
                                pushOutOfBox2 = true;
                            }
                        }
                    }

                    if (pushOutOfBox1 && box.Contains(testPos1, 0f, false))
                    {
                        pushOutOfBox = true;
                        outBox = box;
                    }
                    else if (pushOutOfBox2 && box.Contains(lastPos, 0f, false))
                    {
                        Create_Square(room, lastPos, 10f, 10f, Vec(45), Color.red, 0);

                        pushToLastPos = true;
                        outBox = box;
                    }
                }
            }

            if (snapDist1 < float.MaxValue)
            {
                snapPos = bestSnapPos1;
                surfaceDir = bestSurfaceDir1;

                if (snapPos.y > testPos1.y && pushOutOfBox)
                { contactPoint.y = -1; }
                else if (snapPos.y < testPos2.y && pushOutOfBox)
                { contactPoint.y = 1; }

                if (snapPos.x > pos.x)
                { contactPoint.x = -1; }
                else if (snapPos.x < pos.x)
                { contactPoint.x = 1; }

                return pushOutOfBox;
            }
            else if (snapDist2 < float.MaxValue)
            {
                snapPos = bestSnapPos2;
                surfaceDir = bestSurfaceDir2;

                if (snapPos.y > testPos1.y && pushToLastPos)
                { contactPoint.y = -1; }
                else if (snapPos.y < testPos2.y && pushToLastPos)
                { contactPoint.y = 1; }

                if (snapPos.x > pos.x)
                { contactPoint.x = -1; }
                else if (snapPos.x < pos.x)
                { contactPoint.x = 1; }

                return pushToLastPos;
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
                    Vector2 vert2 = i == box.vertices.Length - 1 ? box.vertices[0] : box.vertices[i + 1];

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
                            //Create_Square(room, relativeTestPos + box.anchorPos, 10f, 10f, Vec(0), Color.blue, 10);
                            //Create_LineBetweenTwoPoints(room, relativeTestPos + box.anchorPos, snapPos + box.anchorPos, 2f, Color.blue, 10);

                            if (bestAccessibility == AItile.Accessibility.Ceiling)
                            { bestAccessibility = AItile.Accessibility.Corridor; }
                            else
                            { bestAccessibility = AItile.Accessibility.Floor; }
                        }
                        else if (dirvec.x > 0.5f || dirvec.x < -0.5f)
                        {
                            //Create_Square(room, relativeTestPos + box.anchorPos, 10f, 10f, Vec(0), Color.red, 10);
                            //Create_LineBetweenTwoPoints(room, relativeTestPos + box.anchorPos, snapPos + box.anchorPos, 2f, Color.red, 10);

                            bestAccessibility = AItile.Accessibility.Wall;
                        }
                        else
                        {
                            //Create_Square(room, relativeTestPos + box.anchorPos, 10f, 10f, Vec(0), Color.green, 10);
                            //Create_LineBetweenTwoPoints(room, relativeTestPos + box.anchorPos, snapPos + box.anchorPos, 2f, Color.green, 10);

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
                box.GetClosestPoint(pos, out Vector2 closestPoint, out float closestPointDist, out int signOfSide);

                //box.GetSnapPosAndContact(pos, pos, 0f, out Vector2 closestPoint, out _, false);
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
