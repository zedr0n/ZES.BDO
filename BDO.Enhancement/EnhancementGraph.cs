using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using BDO.Enhancement.Stochastics;
using QuickGraph;
using QuickGraph.Serialization;
using ZES.Interfaces.Stochastic;

namespace BDO.Enhancement
{
    public class EnhancementGraph
    {
        private readonly BidirectionalGraph<StateVertex, ActionEdge> _graph = new BidirectionalGraph<StateVertex, ActionEdge>();

        public class ActionEdge : IEdge<StateVertex>
        {
            [XmlAttribute("Label")]
            public string Action { get; set; }

            public StateVertex Source { get; set; }
            public StateVertex Target { get; set; }

            public ActionEdge(IMarkovAction<EnhancementState> action)
            {
                Action = action.ToString();
            }
        }
        
        public class StateVertex
        {
            [XmlAttribute("Label")]
            public string Label { get; set; }

            public StateVertex(EnhancementState state)
            {
                Label = state.ToString();
            }
        }

        public EnhancementGraph(IDeterministicPolicy<EnhancementState> policy, EnhancementState initialState, int iterations)
        {
            var nextStates = new HashSet<EnhancementState> { initialState };
            var allStates = new HashSet<EnhancementState>();

            while (iterations-- > 0)
            {
                var nextNextStates = new HashSet<EnhancementState>();
                foreach (var state in nextStates)
                {
                    var from = _graph.Vertices.SingleOrDefault(v => v.Label == state.ToString());
                    if (from == null)
                    {
                        from = new StateVertex(state);
                        _graph.AddVertex(from);
                    }

                    var action = policy[state];
                    if (action == null)
                        continue;

                    var actionStates = action[state].ToArray();
                    foreach (var s in actionStates)
                    {
                        var to = _graph.Vertices.SingleOrDefault(v => v.Label == s.ToString());
                        if (to == null)
                        {
                            to = new StateVertex(s);
                            _graph.AddVertex(to);
                        }

                        _graph.AddEdge(new ActionEdge(action)
                        {
                            Source = from,
                            Target = to,
                        });
                    }
                    
                    nextNextStates.UnionWith(actionStates);
                }
                
                nextStates.Clear();
                foreach (var s in nextNextStates)
                {
                    if (allStates.Add(s))
                        nextStates.Add(s);
                }
            }
        }

        public void Serialize(string filename)
        {
            _graph.SerializeToGraphML<StateVertex, ActionEdge, BidirectionalGraph<StateVertex, ActionEdge>>(filename + ".graphml");;
        }
    }
}