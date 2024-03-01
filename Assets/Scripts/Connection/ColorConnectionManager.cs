using System.Collections.Generic;
using System.Linq;
using Events;
using Player;
using Player.ActionHandlers;
using UnityEngine;

namespace Connection
{
    public class ColorConnectionManager : MonoBehaviour
    {
        [SerializeField] private GameObject colorNodesContainer;
        [SerializeField] private ColorConnector colorConnector;
        
        private readonly ColorConnectionHistoryHandler _historyHandler = new();
        private readonly Dictionary<ColorNodeTarget, bool> _completionsByTargetNode = new();
        private readonly Dictionary<ColorNode, HashSet<ColorNode>> _connectionsFromColorNode = new();
        
        private ColorNode[] _nodes;
        private ClickHandler _clickHandler;
        private ColorNode _currentConnectionMainNode;
        private ColorConnector _currentColorConnector;
        
        private void Awake()
        {
            _nodes = colorNodesContainer.GetComponentsInChildren<ColorNode>();

            var nodeTargets = colorNodesContainer.GetComponentsInChildren<ColorNodeTarget>(true);
            foreach (var nodeTarget in nodeTargets)
            {
                nodeTarget.TargetCompletionChangeEvent += OnTargetCompletionChange;
                _completionsByTargetNode[nodeTarget] = nodeTarget.IsCompleted;
            }

            _clickHandler = ClickHandler.Instance;
            _clickHandler.AddDragEventHandlers(OnDragStart, OnDragEnd);
        }

        private void OnDestroy()
        {
            _clickHandler.ClearEvents();
            _clickHandler.RemoveDragEventHandlers(OnDragStart, OnDragEnd);
        }
        
        private void OnTargetCompletionChange(ColorNodeTarget nodeTarget, bool isCompleted)
        {
            if (!_completionsByTargetNode.ContainsKey(nodeTarget))
                return;

            _completionsByTargetNode[nodeTarget] = isCompleted;

            if (isCompleted && _completionsByTargetNode.Values.All(c => c))
                EventsController.Fire(new EventModels.Game.TargetColorNodesFilled());
        }
        
        private void OnDragStart(Vector3 startPosition)
        {
            if (PlayerController.PlayerState != PlayerState.Connecting)
                return;

            if (TryGetColorNodeInPosition(startPosition, out var colorNode) && !colorNode.IsEmpty)
                StartConnecting(colorNode);
        }

        private void OnDragEnd(Vector3 finishPosition)
        {
            if (PlayerController.PlayerState != PlayerState.Connecting)
                return;

            if (_currentColorConnector == null)
                return;

            if (TryGetColorNodeInPosition(finishPosition, out var colorNode) &&
                _currentColorConnector.CanFinishConnecting &&
                !HaveSameConnection(colorNode) &&
                _currentConnectionMainNode != colorNode)
            {
                FinishConnecting(colorNode);
            }
            else
            {
                CancelConnecting();
            }
            _currentConnectionMainNode = null;
        }
        
        public bool TryGetColorNodeInPosition(Vector2 position, out ColorNode result)
        {
            foreach (var colorNode in _nodes)
            {
                if (!colorNode.IsInBounds(position))
                    continue;

                result = colorNode;
                return true;
            }

            result = null;
            return false;
        }

        private void StartConnecting(ColorNode colorNode)
        {
            _currentColorConnector = Instantiate(colorConnector, colorNode.transform);
            _currentColorConnector.StartConnecting(colorNode.CenterPosition, colorNode.Color);
            _currentColorConnector.gameObject.SetActive(true);

            _currentConnectionMainNode = colorNode;
        }

        private void FinishConnecting(ColorNode colorNode)
        {
            _historyHandler.RememberConnection(_currentConnectionMainNode, colorNode, _currentColorConnector);

            _currentColorConnector.FinishConnecting(colorNode.CenterPosition);
            colorNode.AddColor(_currentColorConnector.Color);

            if (!_connectionsFromColorNode.ContainsKey(_currentConnectionMainNode))
                _connectionsFromColorNode[_currentConnectionMainNode] = new HashSet<ColorNode>();

            _connectionsFromColorNode[_currentConnectionMainNode].Add(colorNode);
        }

        private void CancelConnecting()
        {
            Destroy(_currentColorConnector.gameObject);
        }

        private bool HaveSameConnection(ColorNode colorNode)
        {
            return _connectionsFromColorNode.ContainsKey(_currentConnectionMainNode)
                   && _connectionsFromColorNode[_currentConnectionMainNode].Contains(colorNode)
                   || _connectionsFromColorNode.ContainsKey(colorNode)
                   && _connectionsFromColorNode[colorNode].Count > 0;
        }

        public void ReverseLastConnection()
        {
            var lastConnectionData = _historyHandler.GetPreviousConnectionData();
            if (!lastConnectionData.HasValue)
                return;

            Destroy(lastConnectionData.Value.ColorConnector.gameObject);

            var mainColorNode = lastConnectionData.Value.MainColorNode;
            var targetColorNode = lastConnectionData.Value.TargetColorNode;

            targetColorNode.SetColor(lastConnectionData.Value.PreviousTargetColorNodeColor);
            targetColorNode.SetEmpty(lastConnectionData.Value.WasTargetNodeEmpty);

            _connectionsFromColorNode[mainColorNode].Remove(targetColorNode);
        }
    }
}