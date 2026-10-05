extern alias Firstpass;

using System;
using System.Collections.Generic;
using IL.Watcher;
using JetBrains.Annotations;

namespace ArchdruidsAdditions.Objects
{
    public class CollisionBox : IAccessibilityModifier
    {
        public UpdatableAndDeletable owner;
        public CollisionBoxHandler boxHandler;
        public Vector2[] vertices = [];
        public Vector2[] baseVertices = [];
        public Vector2 cornerPos1, cornerPos2;

        public Vector2 anchorPos, lastAnchorPos;
        public int size, lastSize;
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

                IntVector2 tilePos = owner.room.GetTilePosition(anchorPos);
                IntVector2 lastTilePos = owner.room.GetTilePosition(lastAnchorPos);
                int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = Custom.rotateVectorDeg(baseVertices[i], newRotation);

                    Vector2 vertice1 = anchorPos + vertices[i];
                    Vector2 vertice2 = anchorPos + (i == vertices.Length - 1 ? vertices[0] : vertices[i + 1]);

                    IntVector2 vTilePos = owner.room.GetTilePosition(vertice1);

                    if (vTilePos.x > maxX) { maxX = vTilePos.x; }
                    else if (vTilePos.x < minX) { minX = vTilePos.x; }

                    if (vTilePos.y > maxY) { maxY = vTilePos.y; }
                    else if (vTilePos.y < minY) { minY = vTilePos.y; }

                    Create_LineBetweenTwoPoints(owner.room, vertice1, vertice2, 1f, Color.yellow, 0);
                }

                int sizeX = maxX - minX;
                int sizeY = maxY - minY;

                boxHandler.CallUpdate(tilePos, sizeX, sizeY);
                boxHandler.CallUpdate(lastTilePos, sizeX, sizeY);
            }

            vel = newVel;
        }

        public void ChunkImpact(BodyChunk chunk, float velX, float velY)
        {
            if (owner is PhysicalObject obj)
            {
                foreach (BodyChunk boxChunk in obj.bodyChunks)
                {
                    float dist = Custom.Dist(chunk.pos, boxChunk.pos);

                    boxChunk.vel.x += velX * Mathf.InverseLerp(100, 0, dist) / (boxChunk.mass / chunk.mass);
                    boxChunk.vel.y += velY * Mathf.InverseLerp(100, 0, dist) / (boxChunk.mass / chunk.mass);
                }
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

        public bool[,] baseTilePassages;
        public bool[,] newTilePassages;

        public int[,] baseTerrainProximities;
        public int[,] newTerrainProximities;

        public int[,] baseTileAltitudes;
        public int[,] newTileAltitudes;

        public int updateCollisionCounter = 0;
        public IntVector2 updateTile;

        public bool callForUpdate;

        public List<UpdateTilePosCall> updateTilePosQueue = [];
        public UpdateTilePosCall currentCall;
        public int CombinedTilePosQueLength
        {
            get
            {
                int size = 0;
                foreach (UpdateTilePosCall call in updateTilePosQueue)
                {
                    size += call.sizeX * call.sizeY;
                }
                return size;
            }
        }

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
                        baseTileAltitudes = new int[room.Width, room.Height];
                        baseTilePassages = new bool[room.Width, room.Height];

                        section = 1.2f;

                        for (int i = 0; i < room.Width; i++)
                        {
                            for (int j = 0; j < room.Height; j++)
                            {
                                AItile tile = room.aimap.map[i, j];
                                baseTileAccessibilities[i, j] = tile.acc;
                                baseTileAltitudes[i, j] = tile.floorAltitude;
                                baseTilePassages[i, j] = tile.narrowSpace;

                                baseTerrainProximities[i, j] = room.aimap.terrainProximity[Firstpass.ExtraExtentions.ind(i, j, room.aimap.height)];
                            }
                        }

                        section = 1.3f;

                        newTileAccessibilities = new AItile.Accessibility[room.Width, room.Height];
                        newTerrainProximities = new int[room.Width, room.Height];
                        newTileAltitudes = new int[room.Width, room.Height];
                        newTilePassages = new bool[room.Width, room.Height];
                    }

                    section = 2;

                    if (updateCollisionCounter > 0)
                    {
                        updateCollisionCounter--;
                    }
                    else if (newTileAccessibilities != null && updateTilePosQueue.Count > 0)
                    {
                        if (currentCall == null)
                        {
                            currentCall = updateTilePosQueue[0];
                            updateTile = currentCall.cornerPos;
                        }

                        updateCollisionCounter = 1;

                        for (int i = 0; i < 50; i++)
                        {
                            //Create_Square(room, room.MiddleOfTile(updateTile), 10f, 10f, Vec(45), "Red", 5);

                            AItile.Accessibility newAcc = GetTrueAccessibilityOfTile(updateTile, baseTileAccessibilities[updateTile.x, updateTile.y]);
                            room.aimap.map[updateTile.x, updateTile.y].acc = newAcc;
                            newTileAccessibilities[updateTile.x, updateTile.y] = newAcc;

                            if (baseTerrainProximities != null)
                            {
                                int terrainProximity = Mathf.Min(baseTerrainProximities[updateTile.x, updateTile.y], GetTerrainProximity(updateTile.x, updateTile.y));
                                room.aimap.terrainProximity[Firstpass.ExtraExtentions.ind(updateTile.x, updateTile.y, room.aimap.height)] = terrainProximity;
                                newTerrainProximities[updateTile.x, updateTile.y] = terrainProximity;
                            }

                            if (baseTileAltitudes != null && TryGetSurfacePos(room.MiddleOfTile(updateTile), 0f, out Vector2 surfacePos, out _, out _))
                            {
                                IntVector2 surfaceTilePos = room.GetTilePosition(surfacePos);
                                room.aimap.map[updateTile.x, updateTile.y].floorAltitude = updateTile.y - surfaceTilePos.y;
                                newTileAltitudes[updateTile.x, updateTile.y] = surfaceTilePos.y;
                            }
                            else
                            {
                                int baseAltitude = baseTileAltitudes[updateTile.x, updateTile.y];
                                room.aimap.map[updateTile.x, updateTile.y].floorAltitude = baseAltitude;
                                newTileAltitudes[updateTile.x, updateTile.y] = baseAltitude;
                            }

                            updateTile.x += 1;
                            if (updateTile.x > currentCall.sizeX + currentCall.cornerPos.x)
                            {
                                updateTile.y += 1;
                                if (updateTile.y > currentCall.sizeY + currentCall.cornerPos.y)
                                {
                                    updateTilePosQueue.RemoveAt(0);
                                    currentCall = null;
                                    updateTile = new(0, 0);
                                    break;
                                }
                                updateTile.x = currentCall.cornerPos.x;
                            }
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

        public void CallUpdate(IntVector2 pos, int sizeX, int sizeY)
        {
            if (updateTilePosQueue.Count < 2)
            {
                int newSizeX = sizeX + 10;
                int newSizeY = sizeY + 10;

                if (pos.x > 0 && pos.y > 0 && pos.x + (newSizeX / 2) < room.Width && pos.y + (newSizeY / 2) < room.Height)
                {
                    IntVector2 cornerPos = new(pos.x - (newSizeX / 2), pos.y - (newSizeY / 2));

                    UpdateTilePosCall call = new(cornerPos, newSizeX, newSizeY);
                    updateTilePosQueue.Add(call);
                }
            }

            callForUpdate = true;
        }

        public SharedPhysics.TerrainCollisionData CheckCollisionForBoxes(SharedPhysics.TerrainCollisionData data)
        {
            if (TrySnapToCollisionBox(data.pos, data.lastPos, data.rad, out Vector2 snapPos, out Vector2 surfaceDir, out IntVector2 contactPoint, out CollisionBox box, null))
            {
                if (contactPoint.x != 0 && contactPoint.y != 0)
                {
                    data.pos = snapPos;
                    data.contactPoint = contactPoint;

                    if (box != null)
                    { data.vel.x += box.vel.x; }
                    data.vel.y *= 0f;
                }
            }

            return data;
        }

        public bool TrySnapToCollisionBox(Vector2 pos, Vector2 lastPos, float radius, out Vector2 snapPos, out Vector2 surfaceDir, out IntVector2 contactPoint, out CollisionBox outBox, BodyChunk chunk = null)
        {
            snapPos = pos;
            contactPoint = new(0, 0);
            surfaceDir = Vector2.zero;
            outBox = null;

            CollisionBox closestBox = null;
            float closestBoxDist = float.MaxValue;

            Vector2 vSnapPos = pos;
            float vSnapPosDist = float.MaxValue;
            int vSign = 0;

            Vector2 hSnapPos = pos;
            float hSnapPosDist = float.MaxValue;
            int hSign = 0;

            Vector2 closestSurfaceDir = Vector2.zero;

            Vector2 wcPoint = Vector2.zero;
            float wcPointDist = float.MaxValue;
            IntVector2 wcSignDir = new(0, 0);
            Vector2 wcSurfaceDir = Vector2.zero;

            //Create_LineBetweenTwoPoints(room, Vector2.Lerp(cornerPos, lastPos, 0.5f) + Custom.DirVec(cornerPos, lastPos) * 20f, Vector2.Lerp(cornerPos, lastPos, 0.5f) - Custom.DirVec(cornerPos, lastPos) * 20f, 1f, Color.blue, 0);
            //Create_Square(room, lastPos, 2f, 2f, Vec(45), Color.blue, 0);
            //Create_Square(room, cornerPos, 2f, 2f, Vec(45), Color.red, 0);

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

                        Vector2 rTestPos = rPos;
                        Vector2 rCrossPoint = Custom.LineIntersection(rPos, rLastPos, vertice1, vertice2);

                        if (((rCrossPoint.x <= vertice1.x && rCrossPoint.x >= vertice2.x) || (rCrossPoint.x <= vertice2.x && rCrossPoint.x >= vertice1.x)) &&
                            ((rCrossPoint.y <= vertice1.y && rCrossPoint.y >= vertice2.y) || (rCrossPoint.y <= vertice2.y && rCrossPoint.y >= vertice1.y)) &&
                            ((rCrossPoint.x <= rPos.x && rCrossPoint.x >= rLastPos.x) || (rCrossPoint.x <= rLastPos.x && rCrossPoint.x >= rPos.x)) &&
                            ((rCrossPoint.y <= rPos.y && rCrossPoint.y >= rLastPos.y) || (rCrossPoint.y <= rLastPos.y && rCrossPoint.y >= rPos.y)))
                        {
                            float crossPointDist = Custom.Dist(rCrossPoint, rLastPos);
                            if (crossPointDist < wcPointDist)
                            {
                                wcPoint = rCrossPoint + box.anchorPos;
                                wcPointDist = crossPointDist;

                                if (Mathf.Abs(segmentDir.y) <= 0.5f)
                                {
                                    Vector2 testCollidePos = midPos + new Vector2(0f, -1f) + box.anchorPos;
                                    wcSignDir.y = !box.Contains(testCollidePos, 0f, false) ? 1 : -1;

                                    if (wcSignDir.y < 0)
                                    {
                                        wcSurfaceDir = -Custom.PerpendicularVector(segmentDir);
                                    }
                                }
                                else
                                {
                                    Vector2 testCollidePos = midPos + new Vector2(-1f, 0f) + box.anchorPos;
                                    wcSignDir.x = !box.Contains(testCollidePos, 0f, false) ? 1 : -1;
                                }

                                if (crossPointDist < closestBoxDist)
                                {
                                    closestBoxDist = crossPointDist;
                                    closestBox = box;
                                }
                            }
                            //Create_Square(room, rCrossPoint + box.anchorPos, 6f, 6f, Vec(45), Color.blue, 100);
                        }

                        if (Mathf.Abs(segmentDir.y) <= 0.5f)
                        {
                            Vector2 vCrossPoint = Custom.VerticalCrossPoint(vertice1, vertice2, rTestPos.x);
                            float vCrossPointDist = rTestPos.y - vCrossPoint.y;
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

                                        //Create_Square(room, testCollidePos, 5f, 5f, Vec(45), vSign == 1 ? Color.red : Color.green, 0);

                                        if (vSign < 0)
                                        {
                                            closestSurfaceDir = -Custom.PerpendicularVector(segmentDir);
                                        }

                                        if (vSnapPosDist < closestBoxDist)
                                        {
                                            closestBoxDist = vSnapPosDist;
                                            closestBox = box;
                                        }
                                    }

                                    //Create_Square(room, vCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.red, 0);
                                    //Create_LineBetweenTwoPoints(room, cornerPos, vCrossPoint + box.anchorPos, 1f, Color.red, 0);
                                }
                                else
                                {
                                    //Create_Square(room, vCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.yellow, 0);
                                    //Create_LineBetweenTwoPoints(room, cornerPos, vCrossPoint + box.anchorPos, 1f, Color.yellow, 0);
                                }
                            }
                        }
                        else
                        {
                            Vector2 hCrossPoint = Custom.HorizontalCrossPoint(vertice1, vertice2, rTestPos.y);
                            float hCrossPointDist = rTestPos.x - hCrossPoint.x;
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

                                        if (hSnapPosDist < closestBoxDist)
                                        {
                                            closestBoxDist = hSnapPosDist;
                                            closestBox = box;
                                        }

                                        //Create_Square(room, testCollidePos, 5f, 5f, Vec(45), hSign == 1 ? Color.red : Color.green, 0);
                                    }

                                    //Create_Square(room, hCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.red, 0);
                                    //Create_LineBetweenTwoPoints(room, cornerPos, hCrossPoint + box.anchorPos, 1f, Color.red, 0);
                                }
                                else
                                {
                                    //Create_Square(room, hCrossPoint + box.anchorPos, 4f, 4f, Vec(45), Color.cyan, 0);
                                    //Create_LineBetweenTwoPoints(room, cornerPos, hCrossPoint + box.anchorPos, 1f, Color.cyan, 0);
                                }
                            }
                        }
                    }
                }
            }

            bool pushOutOfBox = false;
            if (wcPointDist < float.MaxValue)
            {
                //Create_Square(room, wcPoint, 6f, 6f, Vec(45), Color.blue, 100);

                snapPos.y = wcPoint.y - radius * wcSignDir.y;
                snapPos.x = wcPoint.x - radius * wcSignDir.x;
                contactPoint.y = wcSignDir.y;
                contactPoint.x = wcSignDir.x;

                if (wcSurfaceDir != Vector2.zero)
                {
                    surfaceDir = wcSurfaceDir;
                }

                pushOutOfBox = true;
            }
            else
            {
                if (vSnapPosDist < float.MaxValue)
                {
                    //Create_Square(room, vSnapPos, 4f, 4f, Vec(45), Color.yellow, 0);
                    //Create_LineBetweenTwoPoints(room, cornerPos, vSnapPos, 1f, Color.yellow, 0);

                    //Create_LineBetweenTwoPoints(room, cornerPos, cornerPos + new Vector2(0f, vSign) * 30f, 2f, Color.red, 0);

                    snapPos.y = vSnapPos.y - radius * vSign;
                    contactPoint.y = vSign;

                    if (closestSurfaceDir != Vector2.zero)
                    {
                        surfaceDir = closestSurfaceDir;

                        //Create_LineBetweenTwoPoints(room, cornerPos + new Vector2(50f, 0f), cornerPos + new Vector2(50f, 0f) + closestSurfaceDir * 30f, 2f, Color.red, 0);
                    }

                    if (closestBox != null)
                    { outBox = closestBox; }

                    pushOutOfBox = true;
                }
                if (hSnapPosDist < float.MaxValue)
                {
                    //Create_Square(room, hSnapPos, 4f, 4f, Vec(45), Color.cyan, 0);
                    //Create_LineBetweenTwoPoints(room, cornerPos, hSnapPos, 1f, Color.cyan, 0);

                    //Create_LineBetweenTwoPoints(room, cornerPos, cornerPos + new Vector2(hSign, 0f) * 30f, 2f, Color.red, 0);

                    snapPos.x = hSnapPos.x - radius * hSign;
                    contactPoint.x = hSign;

                    pushOutOfBox = true;
                }
            }

            if (closestBox != null)
            { outBox = closestBox; }

            return pushOutOfBox;
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

            Vector2 middlePos = room.MiddleOfTile(pos);
            Vector2 floorPos = middlePos + new Vector2(0f, -10f);
            Vector2 ceilPos = middlePos + new Vector2(0f, 10f);
            Vector2 cornerPosNE = middlePos + new Vector2(10f, 10f);
            Vector2 cornerPosNW = middlePos + new Vector2(-10f, 10f);
            Vector2 cornerPosSE = middlePos + new Vector2(10f, -10f);
            Vector2 cornerPosSW = middlePos + new Vector2(-10f, -10f);
            
            bool solidMid = false;
            bool solidFloor = false;
            bool solidCeil = false;
            bool solidNE = false;
            bool solidNW = false;
            bool solidSE = false;
            bool solidSW = false;

            AItile.Accessibility bestAccessibility = baseAcc;
            foreach (CollisionBox box in collisionBoxes)
            {
                bool Mid = box.Contains(middlePos, 0f, false);
                bool Floor = box.Contains(floorPos, 0f, false);
                bool Ceiling = box.Contains(ceilPos, 0f, false);
                bool NE = box.Contains(cornerPosNE, 0f, false);
                bool NW = box.Contains(cornerPosNW, 0f, false);
                bool SE = box.Contains(cornerPosSE, 0f, false);
                bool SW = box.Contains(cornerPosSW, 0f, false);

                if (Mid) { solidMid = true; }
                if (Floor) { solidFloor = true; }
                if (Ceiling) { solidCeil = true; }
                if (NE) { solidNE = true; }
                if (NW) { solidNW = true; }
                if (SE) { solidSE = true; }
                if (SW) { solidSW = true; }
            }

            if (solidNE && solidNW && solidSE && solidSW)
            {
                if (solidMid) { return AItile.Accessibility.Solid; }
                else if (solidFloor) { return AItile.Accessibility.Floor; }
                else { return AItile.Accessibility.Corridor; }
            }

            if (solidCeil || (solidNE && !solidSE) || (solidNW && !solidSW))
            {
                if (baseAcc != AItile.Accessibility.Floor && baseAcc != AItile.Accessibility.CurvedFloor && baseAcc != AItile.Accessibility.Corridor && baseAcc != AItile.Accessibility.Climb)
                { return AItile.Accessibility.Ceiling; }
            }
            if (solidFloor || (solidSE && !solidNE) || (solidSW && !solidNW))
            {
                return AItile.Accessibility.Floor; 
            }

            if ((solidNW && solidSW) || (solidNE && solidSE))
            {
                if (baseAcc == AItile.Accessibility.Wall)
                { return AItile.Accessibility.Corridor; }
                return AItile.Accessibility.Wall;
            }

            return bestAccessibility;
        }

        public bool TryGetSurfacePos(Vector2 pos, float rad, out Vector2 surfacePos, out Vector2 surfaceDir, out float surfacePosDist, bool checkSurfacesAbove = false)
        {
            surfacePos = pos;
            surfacePosDist = 0f;
            surfaceDir = Vector2.up;

            Vector2? closestPos = null;
            float closestDist = float.MaxValue;
            Vector2 closestSurfaceDir = Vector2.up;

            foreach (CollisionBox box in collisionBoxes)
            {
                Vector2 rPos = pos - box.anchorPos;

                for (int i = 0; i < box.vertices.Length; i++)
                {
                    Vector2 vertice1 = box.vertices[i];
                    Vector2 vertice2 = i == box.vertices.Length - 1 ? box.vertices[0] : box.vertices[i + 1];

                    Vector2 segmentDir = Custom.DirVec(vertice1, vertice2);

                    if (Mathf.Abs(segmentDir.y) < 0.6f)
                    {
                        if ((vertice1.x <= rPos.x && vertice2.x >= rPos.x) || (vertice2.x <= rPos.x && vertice1.x >= rPos.x))
                        {
                            Vector2 crossPos = Custom.VerticalCrossPoint(vertice1, vertice2, rPos.x);
                            float dist = rPos.y - crossPos.y;

                            if (box.Contains(new Vector2(crossPos.x, crossPos.y - 1) + box.anchorPos, 0f, false) && (dist > 0 || checkSurfacesAbove) && Mathf.Abs(dist) < closestDist)
                            {
                                closestDist = Mathf.Abs(dist);
                                closestPos = crossPos + box.anchorPos;
                                closestSurfaceDir = Custom.PerpendicularVector(segmentDir) * (vertice1.x < vertice2.x ? 1f : -1f);
                            }
                        }
                    }
                }
            }

            if (closestPos.HasValue)
            {
                surfacePos = new(closestPos.Value.x, closestPos.Value.y + rad);
                surfacePosDist = closestDist;
                return true;
            }
            return false;
        }

        public int GetTerrainProximity(int x, int y)
        {
            Vector2 pos = room.MiddleOfTile(x, y);

            Vector2 nearestBoxPos = new(-10000, -10000);
            float nearestBoxDist = float.MaxValue;
            foreach (CollisionBox box in collisionBoxes)
            {
                box.GetClosestPoint(pos, out Vector2 closestPoint, out float closestPointDist, out int signOfSide);

                //box.GetSnapPosAndContact(cornerPos, cornerPos, 0f, out Vector2 closestPoint, out _, false);
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

        public class UpdateTilePosCall(IntVector2 cornerPos, int sizeX, int sizeY)
        {
            public IntVector2 cornerPos = cornerPos;
            public int sizeX = sizeX;
            public int sizeY = sizeY;
        }
    }
}
