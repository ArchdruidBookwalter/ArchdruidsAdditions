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

            Create_LineBetweenTwoPoints(owner.room, anchorPos, anchorPos + newVel, 1f, Color.green, 0);
        }

        public SharedPhysics.TerrainCollisionData UpdateCollisionData(SharedPhysics.TerrainCollisionData data)
        {
            GetSnapPosAndContact(data.pos, data.lastPos, data.rad, out Vector2 newPos, out IntVector2 contactPoint, false);

            if (contactPoint.x != 0 || contactPoint.y != 0)
            {
                data.pos = newPos;
                data.vel *= 0f;
                data.contactPoint = contactPoint;

                //Create_Square(owner.room, newPos, 20f, 20f, Vec(45), Color.red, 100);
            }

            return data;
        }

        public void GetSnapPosAndContact(Vector2 pos, Vector2 lastPos, float radius, out Vector2 snapPos, out IntVector2 contactPoint, bool visualize)
        {
            Vector2 rPos = pos - anchorPos;
            Vector2 rLastPos = lastPos - anchorPos;

            snapPos = pos;
            contactPoint = new IntVector2(0, 0);

            int lastPosDist = Mathf.Max(Custom.ManhattanDistance(owner.room.GetTilePosition(pos), owner.room.GetTilePosition(lastPos)), 2);

            Vector2 closestSnapPosX = pos;
            Vector2 closestSnapPosY = pos;

            float closestSnapPosXDist = float.MaxValue;
            float closestSnapPosYDist = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vert = vertices[i];
                Vector2 vert2 = i == vertices.Length - 1 ? vertices[0] : vertices[i + 1];

                for (int j = 0; j < lastPosDist; j++)
                {
                    Vector2 rTestPos = Vector2.Lerp(rPos, rLastPos, (float)j / (lastPosDist - 1));

                    Vector2 newSnapPos = Custom.ClosestPointOnLineSegment(vert, vert2, rTestPos);
                    float newSnapPosDist = Custom.Dist(rTestPos, newSnapPos);

                    if (newSnapPos.y == rTestPos.y && newSnapPosDist < closestSnapPosXDist)
                    {
                        closestSnapPosX = newSnapPos + anchorPos;
                        closestSnapPosXDist = newSnapPosDist;
                    }

                    if (newSnapPos.x == rTestPos.x && newSnapPosDist < closestSnapPosYDist)
                    {
                        closestSnapPosY = newSnapPos + anchorPos;
                        closestSnapPosYDist = newSnapPosDist;
                    }
                }
            }

            if (closestSnapPosXDist < radius)
            {
                int sign1 = closestSnapPosX.x > pos.x ? 1 : -1;
                int sign2 = Contains(closestSnapPosX + new Vector2(-sign1, 0) * 5f, 0f, false) ? 1 : -1;

                contactPoint.x = sign2;

                if (visualize)
                { Create_Square(owner.room, closestSnapPosX, 3f, 3f, Vec(45), Color.green, 0); }
            }
            else if (visualize)
            { Create_Square(owner.room, closestSnapPosX, 3f, 3f, Vec(45), Color.red, 0); }

            if (closestSnapPosYDist < radius)
            {
                int sign1 = closestSnapPosY.y > pos.y ? 1 : -1;
                int sign2 = Contains(closestSnapPosY + new Vector2(0, -sign1) * 5f, 0f, false) ? 1 : -1;

                contactPoint.y = sign2;

                if (visualize)
                { Create_Square(owner.room, closestSnapPosY, 3f, 3f, Vec(45), Color.green, 0); }
            }
            else if (visualize)
            { Create_Square(owner.room, closestSnapPosY, 3f, 3f, Vec(45), Color.red, 0); }

            if (closestSnapPosXDist < radius || closestSnapPosYDist < radius)
            {
                snapPos = new(closestSnapPosY.x + contactPoint.y * radius, closestSnapPosX.y + contactPoint.x * radius);
            }
        }

        public bool Contains(Vector2 pos, float rad, bool visualize)
        {
            Vector2 rTestPos = pos - anchorPos;

            int crossings = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vert = vertices[i];
                Vector2 vert2 = i == vertices.Length - 1 ? vertices[0] : vertices[i + 1];

                if ((rTestPos.x <= vert.x && rTestPos.x >= vert2.x) || (rTestPos.x >= vert.x && rTestPos.x <= vert2.x))
                {
                    Vector2 crossPoint = Custom.VerticalCrossPoint(vert, vert2, rTestPos.x);
                    if (crossPoint.y <= rTestPos.y)
                    { crossings++; }
                }
            }

            return crossings % 2 != 0;

            /*
            GetClosestPoint(rTestPos1, out Vector2 closestPoint, out float closestPointDist, out int signOfSide);

            return signOfSide > 0 || Custom.DistLess(pos, closestPoint + anchorPos, rad);*/
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

            int lastPosDist = Mathf.Max(Custom.ManhattanDistance(room.GetTilePosition(pos), room.GetTilePosition(lastPos)), 2);

            CollisionBox standingOnBox = null;
            Vector2 vSnapPos = pos;
            float vSnapPosDist = float.MaxValue;
            int vSign = 0;

            Vector2 hSnapPos = pos;
            float hSnapPosDist = float.MaxValue;
            int hSign = 0;

            Vector2 closestSurfaceDir = Vector2.zero;

            Create_LineBetweenTwoPoints(room, Vector2.Lerp(pos, lastPos, 0.5f) + Custom.DirVec(pos, lastPos) * 20f, Vector2.Lerp(pos, lastPos, 0.5f) - Custom.DirVec(pos, lastPos) * 20f, 1f, Color.blue, 0);
            Create_Square(room, lastPos, 2f, 2f, Vec(45), Color.blue, 0);
            Create_Square(room, pos, 2f, 2f, Vec(45), Color.red, 0);

            foreach (CollisionBox box in collisionBoxes)
            {
                if (chunk == null || chunk.owner != box.owner)
                {
                    Vector2 rPos = pos - box.anchorPos;
                    Vector2 rLastPos = lastPos - box.anchorPos;

                    for (int i = 0; i < box.vertices.Length; i++)
                    {
                        Vector2 vertice1 = box.vertices[i];
                        Vector2 vertice2 = i == box.vertices.Length - 1 ? box.vertices[0] : box.vertices[i + 1];

                        Vector2 segmentDir = Custom.DirVec(vertice1, vertice2);
                        Vector2 midPos = Vector2.Lerp(vertice1, vertice2, 0.5f);

                        if (Mathf.Abs(segmentDir.y) <= 0.5f)
                        {
                            Vector2 vCrossPoint = Custom.VerticalCrossPoint(vertice1, vertice2, rPos.x);
                            float vCrossPointDist = rPos.y - vCrossPoint.y;
                            if ((vCrossPoint.x < vertice1.x && vCrossPoint.x > vertice2.x) || (vCrossPoint.x < vertice2.x && vCrossPoint.x > vertice1.x))
                            {
                                if (Mathf.Abs(vCrossPointDist) < radius)
                                {
                                    if (vCrossPointDist < vSnapPosDist)
                                    {
                                        vSnapPos = vCrossPoint + box.anchorPos;
                                        vSnapPosDist = vCrossPointDist;

                                        Vector2 testCollidePos = midPos + new Vector2(0f, -1f) + box.anchorPos;
                                        vSign = !box.Contains(testCollidePos, 0f, false) ? 1 : -1;

                                        Create_Square(room, testCollidePos, 5f, 5f, Vec(45), vSign == 1 ? Color.red : Color.green, 0);

                                        if (vSign < 0)
                                        {
                                            closestSurfaceDir = -Custom.PerpendicularVector(segmentDir);
                                            standingOnBox = box;
                                        }
                                    }

                                    //Create_Square(room, vCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.red, 0);
                                    //Create_LineBetweenTwoPoints(room, pos, vCrossPoint + box.anchorPos, 1f, Color.red, 0);
                                }
                                else
                                {
                                    //Create_Square(room, vCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.yellow, 0);
                                    //Create_LineBetweenTwoPoints(room, pos, vCrossPoint + box.anchorPos, 1f, Color.yellow, 0);
                                }
                            }
                        }
                        else
                        {
                            Vector2 hCrossPoint = Custom.HorizontalCrossPoint(vertice1, vertice2, rPos.y);
                            float hCrossPointDist = rPos.x - hCrossPoint.x;
                            if ((hCrossPoint.y < vertice1.y && hCrossPoint.y > vertice2.y) || (hCrossPoint.y < vertice2.y && hCrossPoint.y > vertice1.y))
                            {
                                if (Mathf.Abs(hCrossPointDist) < radius)
                                {
                                    if (hCrossPointDist < hSnapPosDist)
                                    {
                                        hSnapPos = hCrossPoint + box.anchorPos;
                                        hSnapPosDist = hCrossPointDist;

                                        Vector2 testCollidePos = midPos + new Vector2(-1f, 0f) + box.anchorPos;
                                        hSign = !box.Contains(testCollidePos, 0f, false) ? 1 : -1;

                                        Create_Square(room, testCollidePos, 5f, 5f, Vec(45), hSign == 1 ? Color.red : Color.green, 0);
                                    }

                                    //Create_Square(room, hCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.red, 0);
                                    //Create_LineBetweenTwoPoints(room, pos, hCrossPoint + box.anchorPos, 1f, Color.red, 0);
                                }
                                else
                                {
                                    //Create_Square(room, hCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.cyan, 0);
                                    //Create_LineBetweenTwoPoints(room, pos, hCrossPoint + box.anchorPos, 1f, Color.cyan, 0);
                                }
                            }
                        }

                        /*
                        Vector2 vCrossPoint = Custom.VerticalCrossPoint(vertice1, vertice2, rPos.x);
                        float vCrossPointDist = rPos.y - vCrossPoint.y;
                        if ((vCrossPoint.x <= vertice1.x && vCrossPoint.x >= vertice2.x) || (vCrossPoint.x <= vertice2.x && vCrossPoint.x >= vertice1.x))
                        {
                            if (Mathf.Abs(vCrossPointDist) < radius)
                            {
                                if (vCrossPointDist < vSnapPosDist)
                                {
                                    vSnapPos = vCrossPoint + box.anchorPos;
                                    vSnapPosDist = vCrossPointDist;

                                    vSign = box.Contains(new Vector2(vSnapPos.x, vSnapPos.y + 5f), 0f, false) ? 1 : -1;

                                    if (vSign < 0)
                                    {
                                        closestSurfaceDir = -Custom.PerpendicularVector(segmentDir);
                                    }
                                }

                                //Create_Square(room, vCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.red, 0);
                                //Create_LineBetweenTwoPoints(room, pos, vCrossPoint + box.anchorPos, 1f, Color.red, 0);
                            }
                            else
                            {
                                //Create_Square(room, vCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.yellow, 0);
                                //Create_LineBetweenTwoPoints(room, pos, vCrossPoint + box.anchorPos, 1f, Color.yellow, 0);
                            }
                        }

                        Vector2 hCrossPoint = Custom.HorizontalCrossPoint(vertice1, vertice2, rPos.y);
                        float hCrossPointDist = rPos.x - hCrossPoint.x;
                        if ((hCrossPoint.y <= vertice1.y && hCrossPoint.y >= vertice2.y) || (hCrossPoint.y <= vertice2.y && hCrossPoint.y >= vertice1.y))
                        {
                            if (Mathf.Abs(hCrossPointDist) < radius)
                            {
                                if (hCrossPointDist < hSnapPosDist)
                                {
                                    hSnapPos = hCrossPoint + box.anchorPos;
                                    hSnapPosDist = hCrossPointDist;

                                    hSign = box.Contains(new Vector2(hSnapPos.x + 5f, hSnapPos.y), 0f, false) ? 1 : -1;
                                }

                                //Create_Square(room, hCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.red, 0);
                                //Create_LineBetweenTwoPoints(room, pos, hCrossPoint + box.anchorPos, 1f, Color.red, 0);
                            }
                            else
                            {
                                //Create_Square(room, hCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.cyan, 0);
                                //Create_LineBetweenTwoPoints(room, pos, hCrossPoint + box.anchorPos, 1f, Color.cyan, 0);
                            }
                        }*/
                    }
                }
            }

            bool pushOutOfBox = false;
            if (vSnapPosDist < float.MaxValue)
            {
                Create_Square(room, vSnapPos, 4f, 4f, Vec(45), Color.yellow, 0);
                Create_LineBetweenTwoPoints(room, pos, vSnapPos, 1f, Color.yellow, 0);

                Create_LineBetweenTwoPoints(room, pos, pos + new Vector2(0f, vSign) * 30f, 2f, Color.red, 0);

                snapPos.y = vSnapPos.y - radius * vSign;
                contactPoint.y = vSign;

                if (closestSurfaceDir != Vector2.zero)
                {
                    surfaceDir = closestSurfaceDir;

                    if (standingOnBox != null)
                    { outBox = standingOnBox; }

                    Create_LineBetweenTwoPoints(room, pos + new Vector2(50f, 0f), pos + new Vector2(50f, 0f) + closestSurfaceDir * 30f, 2f, Color.red, 0);
                }

                pushOutOfBox = true;
            }
            if (hSnapPosDist < float.MaxValue)
            {
                Create_Square(room, hSnapPos, 4f, 4f, Vec(45), Color.cyan, 0);
                Create_LineBetweenTwoPoints(room, pos, hSnapPos, 1f, Color.cyan, 0);

                Create_LineBetweenTwoPoints(room, pos, pos + new Vector2(hSign, 0f) * 30f, 2f, Color.red, 0);

                snapPos.x = hSnapPos.x - radius * hSign;
                contactPoint.x = hSign;

                pushOutOfBox = true;
            }
            return pushOutOfBox;

            /*

            End:;

            if (outBox != null)
            {
                bool lastPosLineCrossesBox = false;
                for (int i = 0; i < lastPosDist; i++)
                {
                    if (outBox.Contains(Vector2.Lerp(pos, lastPos, (float)i / (lastPosDist - 1)), 0f, false))
                    {
                        lastPosLineCrossesBox = true;
                        break;
                    }
                }


                if (lastPosLineCrossesBox || closestSnapPosDist < radius)
                {
                    snapPos = new(pos.x, closestSnapPos.y + radius);
                    surfaceDir = -Custom.PerpendicularVector(closestSurfaceDir);
                    contactPoint.y = -1;

                    //Create_Square(room, snapPos, 5f, 5f, Vec(0), Color.red, 10);
                    //Create_LineBetweenTwoPoints(room, pos, snapPos, 1f, Color.red, 10);

                    return true;
                }
            }
            return false;*/
        }

        public bool TrySnapToCollisionBoxSidesAndBottom(Vector2 pos, Vector2 lastPos, float radius, out Vector2 snapPos, out IntVector2 contactPoint, out CollisionBox outBox, BodyChunk chunk = null)
        {
            snapPos = pos;
            contactPoint = new(0, 0);
            outBox = null;

            Vector2 closestSnapPos = pos;
            float closestSnapPosDist = float.MaxValue;
            float side = 0f;
            Vector2 closestSurfaceDir = Vector2.zero;

            int lastPosDist = Mathf.Max(Custom.ManhattanDistance(room.GetTilePosition(pos), room.GetTilePosition(lastPos)), 2);

            foreach (CollisionBox box in collisionBoxes)
            {
                if (chunk == null || chunk.owner != box.owner)
                {
                    Vector2 rPos = pos - box.anchorPos;
                    Vector2 rLastPos = lastPos - box.anchorPos;

                    for (int i = 0; i < box.vertices.Length; i++)
                    {
                        Vector2 vertice1 = box.vertices[i];
                        Vector2 vertice2 = i == box.vertices.Length - 1 ? box.vertices[0] : box.vertices[i + 1];

                        Vector2 segmentDir = Custom.DirVec(vertice1, vertice2);

                        for (int j = 0; j < lastPosDist; j++)
                        {
                            Vector2 rTestPos = Vector2.Lerp(rPos, rLastPos, (float)j / (lastPosDist - 1));

                            if (vertice1.x <= rTestPos.x && vertice2.x > rTestPos.x || vertice1.x > rTestPos.x && vertice2.x <= rTestPos.x)
                            {
                                Vector2 testSnapPos = Custom.VerticalCrossPoint(vertice1, vertice2, rTestPos.x);

                                /*
                                float testSnapDist = rTestPos.y - testSnapPos.y;

                                if (Mathf.Abs(testSnapDist) < closestSnapPosDist && Mathf.Abs(testSnapDist) < radius)
                                {
                                    closestSnapPosDist = Mathf.Abs(testSnapDist);
                                    closestSnapPos = testSnapPos + box.anchorPos;
                                    side = Mathf.Sign(testSnapDist);
                                    closestSurfaceDir = segmentDir;
                                    outBox = box;
                                }*/

                                Create_Square(room, testSnapPos + box.anchorPos, 5f, 5f, Vec(0), Color.red, 10);
                                Create_LineBetweenTwoPoints(room, pos, testSnapPos + box.anchorPos, 1f, Color.red, 10);
                            }

                            if (vertice1.y <= rTestPos.y && vertice2.y > rTestPos.y || vertice1.y > rTestPos.y && vertice2.y < rTestPos.y)
                            {
                                Vector2 testSnapPos = Custom.HorizontalCrossPoint(vertice1, vertice2, rTestPos.y);

                                /*
                                float testSnapDist = rTestPos.x - testSnapPos.x;

                                if (Mathf.Abs(testSnapDist) < closestSnapPosDist && Mathf.Abs(testSnapDist) < radius)
                                {
                                    closestSnapPosDist = Mathf.Abs(testSnapDist);
                                    closestSnapPos = testSnapPos + box.anchorPos;
                                    side = Mathf.Sign(testSnapDist);
                                    closestSurfaceDir = segmentDir;
                                    outBox = box;
                                }*/

                                Create_Square(room, testSnapPos + box.anchorPos, 5f, 5f, Vec(0), Color.green, 10);
                                Create_LineBetweenTwoPoints(room, pos, testSnapPos + box.anchorPos, 1f, Color.green, 10);
                            }
                        }
                    }
                }
            }

            /*
            if (outBox != null)
            {
                bool lastPosLineCrossesBox = false;
                for (int i = 0; i < lastPosDist; i++)
                {
                    if (outBox.Contains(Vector2.Lerp(pos, lastPos, (float)i / (lastPosDist - 1)), 0f, false))
                    {
                        lastPosLineCrossesBox = true;
                        break;
                    }
                }


                if (lastPosLineCrossesBox || closestSnapPosDist < radius)
                {
                    //snapPos = new(pos.x, closestSnapPos.y + radius);
                    //contactPoint.y = -1;

                    Create_Square(room, snapPos, 5f, 5f, Vec(0), Color.red, 10);
                    Create_LineBetweenTwoPoints(room, pos, snapPos, 1f, Color.red, 10);

                    return false;
                }
            }*/

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
                            //Create_Square(room, rTestPos1 + box.anchorPos, 10f, 10f, Vec(0), Color.blue, 10);
                            //Create_LineBetweenTwoPoints(room, rTestPos1 + box.anchorPos, snapPos + box.anchorPos, 2f, Color.blue, 10);

                            if (bestAccessibility == AItile.Accessibility.Ceiling)
                            { bestAccessibility = AItile.Accessibility.Corridor; }
                            else
                            { bestAccessibility = AItile.Accessibility.Floor; }
                        }
                        else if (dirvec.x > 0.5f || dirvec.x < -0.5f)
                        {
                            //Create_Square(room, rTestPos1 + box.anchorPos, 10f, 10f, Vec(0), Color.red, 10);
                            //Create_LineBetweenTwoPoints(room, rTestPos1 + box.anchorPos, snapPos + box.anchorPos, 2f, Color.red, 10);

                            bestAccessibility = AItile.Accessibility.Wall;
                        }
                        else
                        {
                            //Create_Square(room, rTestPos1 + box.anchorPos, 10f, 10f, Vec(0), Color.green, 10);
                            //Create_LineBetweenTwoPoints(room, rTestPos1 + box.anchorPos, snapPos + box.anchorPos, 2f, Color.green, 10);

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
