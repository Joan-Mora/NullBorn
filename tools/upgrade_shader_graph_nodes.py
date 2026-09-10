#!/usr/bin/env python3
"""Upgrade outdated Shader Graph node versions in .shadergraph / .shadersubgraph files."""

from __future__ import annotations

import json
import sys
from pathlib import Path

# m_Type suffix -> (target sg version, optional default fields to add)
NODE_UPGRADES: dict[str, tuple[int, dict]] = {
    "UnityEditor.ShaderGraph.ViewDirectionNode": (1, {}),
    "UnityEditor.ShaderGraph.PositionNode": (1, {}),
    "UnityEditor.ShaderGraph.GradientNoiseNode": (1, {"m_HashType": 1}),  # LegacyMod
    "UnityEditor.ShaderGraph.SimpleNoiseNode": (1, {}),
    "UnityEditor.ShaderGraph.VoronoiNode": (1, {}),
    "UnityEditor.ShaderGraph.TransformNode": (2, {"m_Normalize": True}),
    "UnityEditor.ShaderGraph.SwizzleNode": (1, {}),
    "UnityEditor.ShaderGraph.CustomFunctionNode": (1, {}),
    "UnityEditor.ShaderGraph.ProceduralVirtualTextureNode": (1, {}),
    "UnityEditor.ShaderGraph.SampleGradientNode": (1, {}),
    "UnityEditor.ShaderGraph.ColorNode": (1, {}),
    "UnityEditor.ShaderGraph.NoiseSineWaveNode": (1, {}),
}

PROPERTY_UPGRADES: dict[str, tuple[int, dict]] = {
    "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.Vector2ShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.Vector3ShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.Vector4ShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.ColorShaderProperty": (3, {}),
    "UnityEditor.ShaderGraph.Internal.SamplerStateShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.Matrix2ShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.Matrix3ShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.Matrix4ShaderProperty": (1, {}),
    "UnityEditor.ShaderGraph.Internal.ShaderKeyword": (2, {}),
}


def split_multi_json(text: str) -> list[str]:
    chunks: list[str] = []
    depth = 0
    start = 0
    for i, ch in enumerate(text):
        if ch == "{":
            if depth == 0:
                start = i
            depth += 1
        elif ch == "}":
            depth -= 1
            if depth == 0:
                chunks.append(text[start : i + 1])
    return chunks


def upgrade_object(obj: dict) -> bool:
    node_type = obj.get("m_Type")
    if not node_type:
        return False

    rules = NODE_UPGRADES.get(node_type) or PROPERTY_UPGRADES.get(node_type)
    if not rules:
        return False

    target_version, defaults = rules
    current = obj.get("m_SGVersion", 0)
    if current >= target_version:
        return False

    obj["m_SGVersion"] = target_version
    for key, value in defaults.items():
        obj.setdefault(key, value)
    return True


def upgrade_file(path: Path) -> int:
    text = path.read_text(encoding="utf-8")
    chunks = split_multi_json(text)
    if not chunks:
        return 0

    upgraded = 0
    out_parts: list[str] = []
    for chunk in chunks:
        obj = json.loads(chunk)
        if upgrade_object(obj):
            upgraded += 1
        out_parts.append(json.dumps(obj, indent=4))

    if upgraded:
        path.write_text("\n\n".join(out_parts) + "\n", encoding="utf-8", newline="\n")
    return upgraded


def main() -> int:
    root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("Assets/_ThirdParty/Water")
    if not root.exists():
        print(f"Path not found: {root}")
        return 1

    files = sorted(root.rglob("*.shadergraph")) + sorted(root.rglob("*.shadersubgraph"))
    total = 0
    for file in files:
        count = upgrade_file(file)
        if count:
            print(f"  {file.relative_to(root.parent.parent.parent)}: {count} nodes/properties upgraded")
            total += count

    print(f"Done. {total} upgrades across {len(files)} files.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
