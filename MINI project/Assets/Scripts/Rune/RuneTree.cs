using System.Collections.Generic;

namespace Tempt
{
    /// <summary>
    /// 한 직업의 룬 트리. 시작 노드 1개 + 자식 분기 다수.
    /// </summary>
    public sealed class RuneTree
    {
        /// <summary>이 트리의 직업.</summary>
        public RuneClass ClassId;

        /// <summary>시작 노드(RequiredRuneId == 0인 노드).</summary>
        public RuneNode Starter;

        /// <summary>모든 노드(ID → RuneNode, 조회용).</summary>
        public Dictionary<int, RuneNode> AllNodes;

        /// <summary>
        /// 데이터에서 직업별 트리를 구성한다.
        /// </summary>
        public static RuneTree BuildFromData(RuneClass classId, IEnumerable<RuneData> allRunes)
        {
            // 동작 요약:
            // - classId에 해당하는 룬 + SubFragment 포함하여 필터링.
            // - RequiredRuneId == 0 인 노드를 Starter로 지정(직업당 1개 보장).
            // - AllNodes를 먼저 Id → RuneNode 로 구성.
            // - 각 RuneData.RequiredRuneId 를 부모 키로 사용해
            //   AllNodes[RequiredRuneId].Next.Add(현재 노드) 로 자식 연결.
            // - 사이클 검사(BFS/DFS).
            //TODO: var tree = new RuneTree { ClassId = classId, AllNodes = new Dictionary<int, RuneNode>() };
            //TODO: foreach (var data in allRunes)
            //TODO: {
            //TODO:     if (data.ClassId != classId && data.RuneType != RuneNodeType.SubFragment) continue;
            //TODO:     var node = new RuneNode { Data = data, Next = new List<RuneNode>() };
            //TODO:     tree.AllNodes[data.Id] = node;
            //TODO:     if (data.RequiredRuneId == 0) tree.Starter = node;
            //TODO: }
            //TODO: // 부모-자식 연결
            //TODO: foreach (var node in tree.AllNodes.Values)
            //TODO: {
            //TODO:     int parentId = node.Data.RequiredRuneId;
            //TODO:     if (parentId != 0 && tree.AllNodes.TryGetValue(parentId, out var parent))
            //TODO:         parent.Next.Add(node);
            //TODO: }
            //TODO: return tree;
            var tree = new RuneTree //Wave0write
            { //Wave0write
                ClassId = classId, //Wave0write
                AllNodes = new Dictionary<int, RuneNode>(), //Wave0write
            }; //Wave0write

            if (allRunes == null) //Wave0write
            { //Wave0write
                return tree; //Wave0write
            } //Wave0write

            foreach (RuneData data in allRunes) //Wave0write
            { //Wave0write
                if (data.ClassId != classId && data.RuneType != RuneNodeType.SubFragment) //Wave0write
                { //Wave0write
                    continue; //Wave0write
                } //Wave0write

                var node = new RuneNode { Data = data, Next = new List<RuneNode>() }; //Wave0write
                tree.AllNodes[data.Id] = node; //Wave0write
                if (data.RequiredRuneId == 0 && data.ClassId == classId && tree.Starter == null) //Wave0write
                { //Wave0write
                    tree.Starter = node; //Wave0write
                } //Wave0write
            } //Wave0write

            foreach (RuneNode node in tree.AllNodes.Values) //Wave0write
            { //Wave0write
                int parentId = node.Data.RequiredRuneId; //Wave0write
                if (parentId != 0 && tree.AllNodes.TryGetValue(parentId, out RuneNode parent)) //Wave0write
                { //Wave0write
                    parent.Next.Add(node); //Wave0write
                } //Wave0write
            } //Wave0write

            return tree; //Wave0write
        }

        /// <summary>
        /// 특정 노드가 해금 가능한지 검사.
        /// 시작 노드는 항상 가능. 그 외에는 부모(RequiredRuneId) 노드가 해금되어 있어야 한다.
        /// </summary>
        public bool CanUnlock(RuneNode node)
        {
            // 동작 요약:
            // - node.Data.RequiredRuneId == 0 이면 시작 노드 → true.
            // - 아니면 AllNodes[node.Data.RequiredRuneId].Unlocked 여부 반환.
            //TODO: if (node.Data.RequiredRuneId == 0) return true;
            //TODO: return AllNodes.TryGetValue(node.Data.RequiredRuneId, out var parent) && parent.Unlocked;
            if (node == null || node.Data == null) //Wave0write
            { //Wave0write
                return false; //Wave0write
            } //Wave0write

            if (node.Data.RequiredRuneId == 0) //Wave0write
            { //Wave0write
                return true; //Wave0write
            } //Wave0write

            return AllNodes != null && AllNodes.TryGetValue(node.Data.RequiredRuneId, out RuneNode parent) && parent.Unlocked; //Wave0write
        }
    }
}

