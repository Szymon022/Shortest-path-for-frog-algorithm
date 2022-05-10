using System;
using ASD.Graphs;
using ASD;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{

    public class Lab06 : System.MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 i 2 - szukanie trasy w nieplynacej rzece
        /// </summary>
        /// <param name="w"> Odległość między brzegami rzeki</param>
        /// <param name="l"> Długość fragmentu rzeki </param>
        /// <param name="lilie"> Opis lilii na rzece </param>
        /// <param name="sila"> Siła żabki - maksymalny kwadrat długości jednego skoku </param>
        /// <param name="start"> Początkowa pozycja w metrach od lewej strony </param>
        /// <returns> (int total, (int x, int y)[] route) - total - suma sił koniecznych do wszystkich skoków, route -
        /// lista par opisujących skoki. Opis jednego skoku (x,y) to dystans w osi x i dystans w osi y, jaki skok pokonuje</returns>
        public (int total, (int, int)[] route) Lab06_FindRoute(int w, int l, int[,] lilie, int sila, int start)
        {
            List<int> end_v;
            int sup_end_v;
            List<(int, int)> punkty = GetPoints(w, l, lilie, start, out sup_end_v, out end_v);
            DiGraph<int> G = TransformToGraph(w, l, punkty, sila, start, end_v, sup_end_v);

            if(G.OutEdges(start) == null)
            {
                return (0, null);
            }


            
            PathsInfo<int> pathsInfo = Paths.Dijkstra(G, start);
            int min_dist;
            int[] path;
            (int, int)[] res_route;
            try
            {
                min_dist = pathsInfo.GetDistance(start, sup_end_v);
                path = pathsInfo.GetPath(start, sup_end_v);

                List<(int, int)> route = new List<(int, int)>();
                Console.WriteLine($"w = {w}, l = {l}");
                (int i, int j) prev_idx = v_to_ij_indexes(path[0], w, l);
                for (int i = 1; i < path.Length; i++)
                {
                    (int i, int j) idx = v_to_ij_indexes(path[i], w, l);
                    Console.WriteLine(idx);
                    route.Add((idx.i - prev_idx.i, idx.j - prev_idx.j));
                    //Console.WriteLine(route[i - 1]);
                    prev_idx = idx;

                }
                res_route = route.ToArray();
            }
            catch(Exception e)
            {
                min_dist = 0;
                res_route = null;
            }

            



            return (min_dist, res_route);
        }

        private int Distance((int x, int y) p1, (int x, int y) p2)
        {
            return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y);
        }

        private List<(int, int)> GetPoints(int w, int l, int[,] lilie, int start, out int sup_end_v, out List<int> end_v)
        {
            List<(int, int)> punkty = new List<(int, int)>();
            end_v = new List<int>();

            // wyluskujemy punkty z tablicy 
            for(int i = 0; i < w; i++)
            {
                for(int j = 0; j < l; j++)
                {
                    if(lilie[i,j] == 1)
                    {
                        punkty.Add((i + 1, j));
                    }
                }
            }

            punkty.Add((0, start));
            for(int j = 0; j < l; j++)
            {
                punkty.Add((w + 1, j));
                end_v.Add((w + 1) * l + j);
            }
            sup_end_v = (w + 1) * l + l;

            return punkty;
        }

        private DiGraph<int> TransformToGraph(int w, int l, List<(int x, int y)> punkty, int sila, int start, List<int> end_v, int sup_end_v)
        {
            DiGraph<int> G = new DiGraph<int>((w + 2) * l + 1);

            int pLen = punkty.Count;

            // polaczenia z punktami
            for(int i = 0; i < pLen; i++)
            {
                (int x, int y) sPoint = punkty[i];
                
                for(int j = 0; j < pLen; j++)
                {
                    (int x, int y) ePoint = punkty[j];
                    int dist = Distance(sPoint, ePoint);
                    
                    if(i != j && dist <= sila)
                    {
                        int u = sPoint.x * l + sPoint.y;
                        int v = ePoint.x * l + ePoint.y;

                        G.AddEdge(u, v, dist);
                    }
                }
            }

            // laczymy end_v z super_end_v
            foreach(int u in end_v)
            {
                G.AddEdge(u, sup_end_v, 0);
            }

            return G;
        }

        private (int i, int j) v_to_ij_indexes(int v, int w, int l)
        {
            return (v / w, v % l);
        }




        /// <summary>
        /// Etap 3 i 4 - szukanie trasy w nieplynacej rzece
        /// </summary>
        /// <param name="w"> Odległość między brzegami rzeki</param>
        /// <param name="l"> Długość fragmentu rzeki </param>
        /// <param name="lilie"> Opis lilii na rzece </param>
        /// <param name="sila"> Siła żabki - maksymalny kwadrat długości jednego skoku </param>
        /// <param name="start"> Początkowa pozycja w metrach od lewej strony </param>
        /// <param name="max_skok"> Maksymalna ilość skoków </param>
        /// <param name="v"> Prędkość rzeki </param>
        /// <returns> (int total, (int x, int y)[] route) - total - suma sił koniecznych do wszystkich skoków, route -
        /// lista par opisujących skoki. Opis jednego skoku (x,y) to dystans w osi x i dystans w osi y, jaki skok pokonuje</returns>
        public (int total, (int, int)[] route) Lab06_FindRouteFlowing(int w, int l, int[,] lilie, int sila, int start, int max_skok, int v)
        {
	    // uzupełnić
            return (0, null);

        }
    }
}
