using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GameCore
{
    [DataContract]
    public class MoveScenarios
    {
        private bool isCaptureScenario;

        [DataMember]
        private Position fromPosition;

        [DataMember]
        private List<List<Position>> allMoveSconarios;



        public MoveScenarios(Position fromPosition, List<Position> scenario)
        {
            this.fromPosition = fromPosition;
            allMoveSconarios = new List<List<Position>>();

            allMoveSconarios.Add(scenario);
        }

        public MoveScenarios(Position fromPosition, List<Position> scenario, bool isCaptureScenario)
        {
            this.fromPosition = fromPosition;
            this.isCaptureScenario = isCaptureScenario;
            allMoveSconarios = new List<List<Position>>();

            allMoveSconarios.Add(scenario);
        }

        public MoveScenarios(Position fromPosition, List<List<Position>> scenarios, bool isCaptureScenario)
        {
            this.fromPosition = fromPosition;
            this.isCaptureScenario = isCaptureScenario;
            this.allMoveSconarios = scenarios;
        }

        public List<List<Position>> getScenarios()
        {
            return allMoveSconarios;
        }

        public List<Position> getScenario(int index)
        {
            return allMoveSconarios[index];
        }

        public Position getFromPosition()
        {
            return fromPosition;
        }

        public bool isCapture()
        {
            return isCaptureScenario;
        }

        public int Count()
        {
            return allMoveSconarios.Count;
        }

        public bool Contains(Position pos, int toPosIndex)
        {
            if (pos.Equals(fromPosition))
                return true;

            foreach (List<Position> scenario in allMoveSconarios)
            {
                for (int i = 0; i < scenario.Count && i < toPosIndex; i++)
                {
                    if (scenario[i].Equals(pos))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public MoveScenarios getMatchScenarios(Position pos, int posIndex)
        {
            List<List<Position>> matchScenarios = new List<List<Position>>();

            foreach (List<Position> scenario in allMoveSconarios)
            {
                if (posIndex < scenario.Count && scenario[posIndex].Equals(pos))
                {
                    matchScenarios.Add(scenario);
                }
            }

            return new MoveScenarios(fromPosition, matchScenarios, isCaptureScenario);
        }

        public MoveScenarios getMatchScenarios(Position pos)
        {
            List<List<Position>> matchScenarios = new List<List<Position>>();

            foreach (List<Position> scenario in allMoveSconarios)
            {
                foreach (Position p in scenario)
                {
                    if (p.Equals(pos))
                    {
                        matchScenarios.Add(scenario);
                        break;
                    }
                }
            }

            return new MoveScenarios(fromPosition, matchScenarios, isCaptureScenario);
        }

        public void print()
        {
            Console.WriteLine(fromPosition + ":");

            foreach (List<Position> scenario in allMoveSconarios)
            {
                foreach (Position p in scenario)
                {
                    Console.Write(p + "->");
                }
                Console.WriteLine();
            }
        }
    }
}
