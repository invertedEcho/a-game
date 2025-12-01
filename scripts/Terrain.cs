using System;
using Godot;

public partial class Terrain : Node3D {
    [Export]
    MeshInstance3D terrainMesh;

    [Export]
    MultiMeshInstance3D treeMeshes;

    const int instanceCount = 350;
    const float Offset = 10f;
    const float HalfOfOffset = Offset / 2;
    const float MinScaleValue = 0.5f;
    const float MaxScalueValue = 1.5f;
    const float MinRotationValue = 0.0f;
    const float MaxRotationValue = Mathf.Pi * 2;

    public override void _Ready() {
        SpawnRandomTreesAtEdgesOfMap();
    }

    private void SpawnRandomTreesAtEdgesOfMap() {
        treeMeshes.Multimesh.InstanceCount = instanceCount;
        treeMeshes.Multimesh.Mesh = GD.Load<Mesh>("res://assets/models/nature/pine_tree_mesh.tres");

        Random randomInstance = new Random();

        Aabb boundingBox = terrainMesh.GetAabb();

        // we decrease the size, because it shouldnt be at the very edge but a bit before
        float boundingBoxSizeX = boundingBox.Size.X - 30f;
        float boundingBoxSizeZ = boundingBox.Size.Z - 30f;

        float globalMinX = -(boundingBoxSizeX / 2f);
        float globalMaxX = (boundingBoxSizeX / 2f);

        float globalMinZ = -(boundingBoxSizeZ / 2f);
        float globalMaxZ = (boundingBoxSizeZ / 2f);

        float lowerMinX = globalMinX - HalfOfOffset;
        float upperMinX = globalMinX + HalfOfOffset;

        float lowerMaxX = globalMaxX - HalfOfOffset;
        float upperMaxX = globalMaxX + HalfOfOffset;

        float lowerMinZ = globalMinZ - HalfOfOffset;
        float upperMinZ = globalMinZ + HalfOfOffset;

        float lowerMaxZ = globalMaxZ - HalfOfOffset;
        float upperMaxZ = globalMaxZ + HalfOfOffset;

        int currentMeshInstanceIndex = 0;

        // 1. X: between global
        //    Z: 0, but has to be like - 5 and + 5 to have slight derivation
        for (int i = 0; i < instanceCount / 4; i++) {
            float randomX = MathUtils.GetRandomFloatRange(randomInstance, globalMinX, globalMaxX);
            float randomZ = MathUtils.GetRandomFloatRange(randomInstance, lowerMinZ, upperMinZ);

            Vector3 randomEdgePoint = new Vector3((float)randomX, 5f, (float)randomZ);
            Vector3? snappedEdgePoint = SnapToTerrain(randomEdgePoint);
            if (snappedEdgePoint != null) {
                Transform3D transform = Transform3D.Identity;
                transform.Origin = (Vector3)snappedEdgePoint;
                treeMeshes.Multimesh.SetInstanceTransform(currentMeshInstanceIndex, transform);
            }
            currentMeshInstanceIndex++;
        }

        // X: between globalMinX and globalMaxX
        // Z: maxZ, but with -HalfOfOffset and +HalfOfOffset
        for (int i = 0; i < instanceCount / 4; i++) {
            float randomX = MathUtils.GetRandomFloatRange(randomInstance, globalMinX, globalMaxX);
            double randomZ = MathUtils.GetRandomFloatRange(randomInstance, lowerMaxZ, upperMaxZ);

            Vector3 randomEdgePoint = new Vector3((float)randomX, 5f, (float)randomZ);
            Vector3? snappedEdgePoint = SnapToTerrain(randomEdgePoint);
            if (snappedEdgePoint != null) {
                Transform3D transform = Transform3D.Identity;
                transform.Origin = (Vector3)snappedEdgePoint;
                treeMeshes.Multimesh.SetInstanceTransform(currentMeshInstanceIndex, transform);
            }
            currentMeshInstanceIndex++;
        }

        for (int i = 0; i < instanceCount / 4; i++) {
            float randomX = MathUtils.GetRandomFloatRange(randomInstance, lowerMinX, upperMinX);
            double randomZ = MathUtils.GetRandomFloatRange(randomInstance, globalMinZ, globalMaxZ);

            Vector3 randomEdgePoint = new Vector3((float)randomX, 5f, (float)randomZ);
            Vector3? snappedEdgePoint = SnapToTerrain(randomEdgePoint);
            if (snappedEdgePoint != null) {
                Transform3D transform = Transform3D.Identity;
                transform.Origin = (Vector3)snappedEdgePoint;
                treeMeshes.Multimesh.SetInstanceTransform(currentMeshInstanceIndex, transform);
            }
            currentMeshInstanceIndex++;
        }

        for (int i = 0; i < instanceCount / 4; i++) {
            float randomX = MathUtils.GetRandomFloatRange(randomInstance, lowerMaxX, upperMaxX);
            double randomZ = MathUtils.GetRandomFloatRange(randomInstance, globalMinZ, globalMaxZ);

            Vector3 randomEdgePoint = new Vector3((float)randomX, 5f, (float)randomZ);
            Vector3? snappedEdgePoint = SnapToTerrain(randomEdgePoint);
            if (snappedEdgePoint != null) {
                Transform3D transform = Transform3D.Identity;
                transform.Origin = (Vector3)snappedEdgePoint;
                treeMeshes.Multimesh.SetInstanceTransform(currentMeshInstanceIndex, transform);
            }
            currentMeshInstanceIndex++;
        }

        for (int i = 0; i < instanceCount; i++) {
            float randomScaleValue = MathUtils.GetRandomFloatRange(randomInstance, MinScaleValue, MaxScalueValue);
            float randomRotationValue = MathUtils.GetRandomFloatRange(randomInstance, MinRotationValue, MaxRotationValue);
            GD.Print($"randomScaleValue: {randomScaleValue}");

            Transform3D existingTransform = treeMeshes.Multimesh.GetInstanceTransform(i);

            Basis newBasis = existingTransform.Basis;
            newBasis = newBasis.Scaled(new Vector3(randomScaleValue, randomScaleValue, randomScaleValue));
            newBasis = newBasis.Rotated(new Vector3(0f, 1f, 0f), randomRotationValue);
            existingTransform.Basis = newBasis;
            treeMeshes.Multimesh.SetInstanceTransform(i, existingTransform);
        }
    }

    public Vector3? SnapToTerrain(Vector3 position) {
        var space = GetWorld3D().DirectSpaceState;

        Vector3 start = position + Vector3.Up * 200.0f;
        Vector3 end = position + Vector3.Down * 200.0f;

        PhysicsRayQueryParameters3D queryParameters = new PhysicsRayQueryParameters3D();
        queryParameters.From = start;
        queryParameters.To = end;

        var rayCastResult = space.IntersectRay(queryParameters);

        Variant position2;
        if (rayCastResult.TryGetValue("position", out position2)) {
            GD.Print($"found top of position: {position2}");
            return (Vector3)position2;
        }
        return null;
    }
}
