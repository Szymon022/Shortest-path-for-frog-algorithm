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
            //List<int> end_v;
            int start_v;
            int end_v;
            List<(int, int)> punkty = GetPoints(w, l, lilie);
            DiGraph<int> G = TransformToGraph(w, l, punkty, sila, start, out start_v, out end_v);

            if(G.OutEdges(start) == null)
            {
                return (0, null);
            }


            
            PathsInfo<int> pathsInfo = Paths.Dijkstra(G, start_v);
            int min_dist;
            int[] path;
            (int, int)[] res_route = null;
            try
            {
                min_dist = pathsInfo.GetDistance(start_v, end_v);
                path = pathsInfo.GetPath(start_v, end_v);
                if(path.Length == 2) // bezposredni skok na brzeg
                {
                    (int, int) s_p = (-1, start);
                    (int, int) e_p = (w /** (w - p_last.Item1)*/, 0);
                    (int, int)[] result = { (e_p.Item1 - s_p.Item1, 0) };
                    return (min_dist, result);
                }

                // wpolzedne, ktore nie sa w tablicy lilie
                (int, int) start_p = (-1, start);
                // przedostatni punkt to ostatni punkt ktory znajduje sie w tablicy lilie
                (int, int) p_last = v_to_ij_indexes(path[path.Length - 2], w, l);
                // ostatnie wspolrzedne
                (int, int) end_p = ( (w - p_last.Item1) /** (w - p_last.Item1)*/, 0);

                
                List<(int, int)> route = new List<(int, int)>();

                (int i, int j) prev_idx = start_p;
                int i;
                for (i = 1; i < path.Length - 1; i++)
                {
                    (int i, int j) idx = v_to_ij_indexes(path[i], w, l);
                    //Console.WriteLine(idx);
                    route.Add((idx.i - prev_idx.i, idx.j - prev_idx.j));
                    //Console.WriteLine(route[i - 1]);
                    prev_idx = idx;

                }
                route.Add(end_p);
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

        private List<(int, int)> GetPoints(int w, int l, int[,] lilie)
        {
            List<(int, int)> punkty = new List<(int, int)>();
            
            // wyluskujemy punkty z tablicy 
            for(int i = 0; i < w; i++)
            {
                for(int j = 0; j < l; j++)
                {
                    if(lilie[i,j] == 1)
                    {
                        punkty.Add((i, j));
                    }
                }
            }
            return punkty;
        }

        private DiGraph<int> TransformToGraph(int w, int l, List<(int x, int y)> punkty, int sila, int start, out int start_v, out int end_v)
        {
            // w * l miejsc w tablicy lilie + 2 wierzchołki - start_v oraz super end_v
            DiGraph<int> G = new DiGraph<int>(w * l + 2);

            int pLen = punkty.Count;

            // polaczenia z punktami
            for(int i = 0; i < pLen; i++)
            {
                (int i, int j) sPoint = punkty[i];
                
                for(int j = 0; j < pLen; j++)
                {
                    (int i, int j) ePoint = punkty[j];
                    int dist = Distance(sPoint, ePoint);
                    
                    if(i != j && dist <= sila)
                    {
                        int u = sPoint.i * l + sPoint.j;
                        int v = ePoint.i * l + ePoint.j;

                        G.AddEdge(u, v, dist);
                    }
                }
            }
            

            // laczymy start ze wszystkimi mozliwymi wierzcholkami
            start_v = w * l;
            (int, int) s_v = (-1, start); // wiersz o numerze -1, bo to jest poza tablica
            end_v = w * l + 1;
            foreach ((int i, int j) p in punkty)
            {
                int dist = Distance(p, s_v);
                if (dist <= sila)
                {
                    int u = p.i * l + p.j;
                    G.AddEdge(start_v, u, dist);
                }
                // laczymy wzsystkie mozliwe wierzcholki z koncami
                // dystans od brzegu
                dist = (w - p.i) * (w - p.i);
                if (dist <= sila)
                {
                    int u = p.i * l + p.j;
                    G.AddEdge(u, end_v, dist);
                }
            }
            int dist_directly_to_end = (w + 1) * (w + 1);
            if (dist_directly_to_end <= sila)
            {
                G.AddEdge(start_v, end_v, dist_directly_to_end);
            }
            //Console.WriteLine(G.ToString());
            return G;
        }

        private (int i, int j) v_to_ij_indexes(int v, int w, int l)
        {
            return (v / l, v % l);
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
            // w * l wierzcholkow na kazdy poziom, max_skok poziomow, max_skok startow, 1 end
            // [0 ; w*l*max_skok - 1] -> indeksy wierzcholkow
            // [w*l*max_skok; w*l*max_skok + max_skok] -> indeksy startow
            // w*l*max_skok + max_skok + 1 -> indeks konca
            DiGraph<int> G = new DiGraph<int>(w * l * max_skok + max_skok + 2);

            List<(int, int)>[] levels_points = new List<(int, int)>[max_skok];
            // for each level of graph (max_skok amount of levels)
            //printTab2D(lilie);
            int[,] shiftedLilies = lilie;
            for(int i = 0; i < max_skok; i++)
            {
                shiftedLilies = ShiftToRight(shiftedLilies, v);
                //printTab2D(shiftedLilies);

                levels_points[i] = GetPoints(w, l, shiftedLilies);
                
            }
            int start_v;
            int end_v;

            MakeConnections(G, levels_points, w, l, sila, start, out start_v, out end_v);

            PathsInfo<int> pathsInfo = Paths.Dijkstra<int>(G, start_v);
            if(pathsInfo.Reachable(start_v, end_v))
            {
               
                int[] path = pathsInfo.GetPath(start_v, end_v);

                //for (int k = 0; k < path.Length; k++)
                //{
                //    Console.Write($"{path[k]}, ");
                //}
                // wpolzedne, ktore nie sa w tablicy lilie
                (int, int) start_p = (-1, start);
                // przedostatni punkt to ostatni punkt ktory znajduje sie w tablicy lilie
                (int, int, int) p_last = v_to_ij_level_indexes(path[path.Length - 2], w, l, max_skok, start);
                // ostatnie wspolrzedne
                (int, int) end_p = ((w - p_last.Item1) /* * (w - p_last.Item1)*/, 0); // tutaj chyba bez kwadratu


                List<(int, int)> route = new List<(int, int)>();

                (int i, int j) prev_idx = start_p;
                int i;
                for (i = 1; i < path.Length - 1; i++)
                {
                    (int i, int j, int) idx = v_to_ij_level_indexes(path[i], w, l, max_skok, start); // gdy przechodzimy do startu, to ruch powinien byc (0,0)
                    //Console.WriteLine(idx);
                    route.Add((idx.i - prev_idx.i, idx.j - prev_idx.j));
                    //Console.WriteLine(route[i - 1]);
                    prev_idx = (idx.i, idx.j);

                }
                route.Add(end_p);

                return (pathsInfo.GetDistance(start_v, end_v), route.ToArray());
            }
            
            return (0, null);

        }

        private void printTab2D(int [,] tab2D)
        {
            int w = tab2D.GetLength(0);   //height
            int l = tab2D.GetLength(1);   //width

            Console.WriteLine();
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < l; j++)
                {
                    Console.Write($"{tab2D[i, j]},");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private int[,] ShiftToRight(int[,] tab, int v)
        {
            // create copy of tab
            int w = tab.GetLength(0);   //height
            int l = tab.GetLength(1);   //width
            int[,] shiftedTab = new int[w, l];

            for(int i = 0; i < w; i++)
            {
                for(int j = 0; j < l; j++)
                {
                    if(tab[i, j] != 0)
                    {
                        shiftedTab[i, (j + v) % l] = tab[i, j];
                    }
                }
            }

            //Console.Write("\n");
            //for (int i = 0; i < w; i++)
            //{
            //    for (int j = 0; j < l; j++)
            //    {
            //        Console.Write(shiftedTab[i, j] + " ");
            //    }
            //    Console.Write("\n");
            //}

            return shiftedTab;
        }

        private void MakeConnections(DiGraph<int> G, List<(int,int)>[] levels_points, int w, int l, int sila, int start, out int start_v, out int end_v)
        {
            int max_skok = levels_points.Length;
            end_v = w * l * max_skok + max_skok + 1;
            start_v = w * l * max_skok;

            int s_v = start_v;
            int next_s_v = s_v + 1;
            (int i, int j) s_v_point = (-1, start);
            foreach ((int i, int j) p in levels_points[0])
            {
                int dist = Distance(s_v_point, p);
                if (dist <= sila)
                {
                    int u = p.i * l + p.j;
                    G.AddEdge(s_v, u, dist);
                }
            }
            G.AddEdge(s_v, next_s_v, 0);


            // for each level connect it with the previous one
            for (int i = 1; i < max_skok; i++)
            {
                // poprzednia plansza
                List<(int i, int j)> prev_level = levels_points[i - 1];
                // obecna plansza
                List<(int i, int j)> level = levels_points[i];

                // lacze wszystkie wierzcholki z prev_level_point z koncem
                int e_v = end_v;
                

                
                foreach ((int i, int j) prev_level_point in prev_level)
                {
                    // lacze punkty z poprzedniej planszy z koncem
                    int dist = (w - prev_level_point.i) * (w - prev_level_point.i);
                    if (dist <= sila)
                    {
                        int u = prev_level_point.i * l + prev_level_point.j + (i - 1) * w * l;
                        G.AddEdge(u, e_v, dist);
                    }

                    // laczymy punkty poprzedniej planszy z obecna
                    foreach ((int i, int j) level_point in level)
                    {
                        
                        dist = Distance(prev_level_point, level_point);
                        if(dist <= sila)
                        {
                            int u = prev_level_point.i * l + prev_level_point.j + (i - 1) * w * l;
                            int v = level_point.i * l + level_point.j + i * w * l;
                            
                            G.AddEdge(u, v, dist);
                        }
                    }
                }
                // lacze wierzcholek startowy z kazdym punktem z planszy wyzej
                s_v = w * l * max_skok + i;// - 1;
                next_s_v = w * l * max_skok + i + 1;
                s_v_point = (-1, start);
                foreach((int i, int j) level_point in level)
                {
                    int dist = Distance(s_v_point, level_point);
                    if(dist <= sila)
                    {
                        int u = level_point.i * l + level_point.j + i * w * l;

                        G.AddEdge(s_v, u, dist);
                    }
                }
                G.AddEdge(s_v, next_s_v, 0);

                // lacze wierzcholek startowy z koncem jesli sie da

                int dist_directly_to_end = (w+1) * (w+1);
                if(dist_directly_to_end <= sila)
                {
                    G.AddEdge(s_v, end_v, dist_directly_to_end);
                }


            }

            


        }

        private (int i, int j, int level) v_to_ij_level_indexes(int v, int w, int l, int levels, int start)
        {
            int level = v / (w * l);
            if (v >= w*l*levels && v <= w*l*levels + levels) // jestesmy w wierzcholku startowym
            {
                return (-1, start, level);
            }

            v = v % (w * l);

            int i = v / l;
            int j = v % l;

            return (i, j, level);
        }
    }
}
