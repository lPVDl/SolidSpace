using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.EntitySearch;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Capture
{
    internal class CaptureTool : ICaptureTool
    {
        private const int GizmosSquareSize = 6;
        
        public IEntitySearchSystem SearchSystem { get; set; }
        public IComponentFilter Filter { get; set; }
        public IPlaygroundUIManager PlaygroundUI { get; set; }
        public IUIManager UIManager { get; set; }
        public IPointerTracker Pointer { get; set; }
        public List<Entity> CapturedEntities { get; set; }
        public List<float2> CapturedPositions { get; set; }
        public float2 CapturedPointer { get; set; }
        public Color GizmosColor { get; set; }
        public GizmosHandle Gizmos { get; set; }
        public int SearchRadius { get; set; }
        public IStringField SearchRadiusField { get; set; }
        public IEntityWorldManager EntityManager { get; set; }
        public ICaptureToolHandler Handler { get; set; }
        public IPlaygroundToolValueStorage ValueStorage { get; set; }
        
        public IToolWindow Window { get; set; }

        public void OnActivate(bool isActive)
        {
            if (isActive)
            {
                UpdateSearchSystemQuery();
                CapturedEntities.Clear();
                CapturedPositions.Clear();
                SearchRadius = (int) ValueStorage.GetValueOrDefault("InteractionRange");
                SearchRadius = Math.Max(0, SearchRadius);
                SearchRadiusField.SetValue(SearchRadius.ToString());
                SearchSystem.SetSearchRadius(SearchRadius);
            }
            else
            {
                ValueStorage.SetValue("InteractionRange", SearchRadius);
            }
            
            PlaygroundUI.SetElementVisible(Filter, isActive);
            PlaygroundUI.SetElementVisible(Window, isActive);
            SearchSystem.SetEnabled(isActive);
        }

        public void OnUpdate()
        {
            if (CapturedEntities.Count > 0)
            {
                OnCaptureUpdate();

                if (CapturedEntities.Count != 0)
                {
                    return;
                }
            }
            
            if (UIManager.IsMouseOver)
            {
                return;
            }
            
            SearchSystem.SetSearchPosition(Pointer.Position);
            
            if (SearchRadius == 0)
            {
                OnSingleSearchUpdate();
            }
            else
            {
                OnRadiusSearchUpdate();
            }
        }

        public EntityQuery CreateQueryFromCurrentFilter()
        {
            return EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = Filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = Filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
        }

        private void OnCaptureUpdate()
        {
            var eventData = new CaptureEventData
            {
                eventType = ECaptureEventType.Update,
                currentPointer = Pointer.Position,
                startPointer = CapturedPointer,
            };

            for (var i = CapturedEntities.Count - 1; i >= 0; i--)
            {
                var entity = CapturedEntities[i];
                if (!EntityManager.CheckExists(entity))
                {
                    CapturedEntities.RemoveAt(i);
                    CapturedPositions.RemoveAt(i);
                    continue;
                }

                var entityPosition = CapturedPositions[i];
                eventData.entity = entity;
                eventData.startEntityPosition = entityPosition;
                Handler.OnCaptureEvent(eventData);
                
                Gizmos.DrawScreenSquare(entityPosition, GizmosSquareSize, GizmosColor);
            }

            if (Pointer.IsHeldThisFrame)
            {
                return;
            }

            eventData.eventType = ECaptureEventType.End;
            for (var i = 0; i < CapturedEntities.Count; i++)
            {
                eventData.entity = CapturedEntities[i];
                eventData.startEntityPosition = CapturedPositions[i];
                Handler.OnCaptureEvent(eventData);
            }
            
            CapturedEntities.Clear();
            CapturedPositions.Clear();
        }

        private void OnSingleSearchUpdate()
        {
            var searchResult = SearchSystem.Result;
            if (!searchResult.isValid)
            {
                return;
            }
            
            Gizmos.DrawLine(Pointer.Position, searchResult.nearestPosition, GizmosColor);
            Gizmos.DrawScreenSquare(searchResult.nearestPosition, GizmosSquareSize, GizmosColor);

            if (!Pointer.ClickedThisFrame)
            {
                return;
            }
            
            CapturedEntities.Add(searchResult.nearestEntity);
            CapturedPositions.Add(searchResult.nearestPosition);
            CapturedPointer = Pointer.Position;
                
            Handler.OnCaptureEvent(new CaptureEventData
            {
                eventType = ECaptureEventType.Start,
                entity = searchResult.nearestEntity,
                startEntityPosition = searchResult.nearestPosition,
                currentPointer = Pointer.Position,
                startPointer = Pointer.Position
            });
        }
        
        private void OnRadiusSearchUpdate()
        {
            Gizmos.DrawWirePolygon(Pointer.Position, SearchRadius, 64, GizmosColor);
            
            var searchResult = SearchSystem.Result;
            if (!searchResult.isValid)
            {
                return;
            }
            
            for (var i = 0; i < searchResult.inRadiusCount; i++)
            {
                var entityPosition = searchResult.inRadiusPositions[i];
                Gizmos.DrawScreenSquare(entityPosition, GizmosSquareSize, GizmosColor);
            }

            if (!Pointer.ClickedThisFrame || !EntityManager.CheckExists(searchResult.nearestEntity))
            {
                return;
            }

            CapturedPointer = Pointer.Position;
            var eventData = new CaptureEventData
            {
                startPointer = Pointer.Position,
                currentPointer = Pointer.Position,
                eventType = ECaptureEventType.Start
            };

            for (var i = 0; i < searchResult.inRadiusCount; i++)
            {
                var entity = searchResult.inRadiusEntities[i];
                if (!EntityManager.CheckExists(entity))
                {
                    continue;
                }
                
                var position = searchResult.inRadiusPositions[i];
                CapturedEntities.Add(entity);
                CapturedPositions.Add(position);
                eventData.entity = entity;
                eventData.startEntityPosition = position;
                Handler.OnCaptureEvent(eventData);
            }
        }

        public void OnSearchRadiusFieldChange()
        {
            SearchRadius = int.Parse(SearchRadiusField.Value);
            SearchSystem.SetSearchRadius(SearchRadius);
        }
        
        public void UpdateSearchSystemQuery()
        {
            SearchSystem.SetQuery(new EntityQueryDesc
            {
                All = Filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = Filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
        }
    }
}