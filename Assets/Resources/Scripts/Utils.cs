﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public const int SIZE_GRID = 19;
    public const int VICTORY = 200000000;
    public const int DEFEAT = -199999999;

    public static readonly Dictionary<ushort, int> PatternsIA
    = new Dictionary<ushort, int>
    {
        // Off
        // 5 pions alignés
        { (ushort)((2 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 2), VICTORY },    // 2_2_2_2_2

        // 4 pions alignés
        { (ushort)((2 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 0), 4000000 },      // 2_2_2_2_0
        { (ushort)((0 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 2), 4000000 },      // 0_2_2_2_2

        // 3 pions alignés
        { (ushort)((2 << 8) + (2 << 6) + (2 << 4) + (0 << 2) + 0), 320000 },
        { (ushort)((0 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 0), 320000 },
        { (ushort)((0 << 8) + (0 << 6) + (2 << 4) + (2 << 2) + 2), 320000 },

        // pions seul
        { (ushort)((2 << 8) + (0 << 6) + (0 << 4) + (0 << 2) + 0), 1 },
        { (ushort)((0 << 8) + (2 << 6) + (0 << 4) + (0 << 2) + 0), 1 },
        { (ushort)((0 << 8) + (0 << 6) + (2 << 4) + (0 << 2) + 0), 1 },
        { (ushort)((0 << 8) + (0 << 6) + (0 << 4) + (2 << 2) + 0), 1 },
        { (ushort)((0 << 8) + (0 << 6) + (0 << 4) + (0 << 2) + 2), 1 },

        // 4 pions séparés
        { (ushort)((2 << 8) + (0 << 6) + (2 << 4) + (2 << 2) + 2), 2900000 },
        { (ushort)((2 << 8) + (2 << 6) + (0 << 4) + (2 << 2) + 2), 2900000 },
        { (ushort)((2 << 8) + (2 << 6) + (2 << 4) + (0 << 2) + 2), 2900000 },

        // 3 pions séparés
        { (ushort)((2 << 8) + (0 << 6) + (2 << 4) + (0 << 2) + 2), 250000 },       // 2_0_2_0_2
        { (ushort)((2 << 8) + (0 << 6) + (2 << 4) + (2 << 2) + 0), 250000 },       // 2_0_2_2_0
        { (ushort)((0 << 8) + (2 << 6) + (0 << 4) + (2 << 2) + 2), 250000 },       // 0_2_0_2_2
        { (ushort)((2 << 8) + (0 << 6) + (0 << 4) + (2 << 2) + 2), 250000 },       // 2_0_0_2_2
        { (ushort)((2 << 8) + (2 << 6) + (0 << 4) + (0 << 2) + 2), 250000 },       // 2_2_0_0_2
        { (ushort)((0 << 8) + (2 << 6) + (2 << 4) + (0 << 2) + 2), 250000 },       // 0_2_2_0_2
        { (ushort)((2 << 8) + (2 << 6) + (0 << 4) + (2 << 2) + 0), 250000 },       // 2_2_0_2_0
        
        // 2 pions alignés
        { (ushort)((2 << 8) + (2 << 6) + (0 << 4) + (0 << 2) + 0), 15 },        // 2_2_0_0_0
        { (ushort)((0 << 8) + (2 << 6) + (2 << 4) + (0 << 2) + 0), 15 },        // 0_2_2_0_0
        { (ushort)((0 << 8) + (0 << 6) + (2 << 4) + (2 << 2) + 0), 15 },        // 0_0_2_2_2
        { (ushort)((0 << 8) + (0 << 6) + (0 << 4) + (2 << 2) + 2), 15 },        // 0_0_0_2_2
        // pions séparés, 2 pions séparés
        { (ushort)((2 << 8) + (0 << 6) + (2 << 4) + (0 << 2) + 0), 10 },        // 2_0_2_0_0
        { (ushort)((2 << 8) + (0 << 6) + (0 << 4) + (0 << 2) + 2), 10 },        // 2_0_0_0_2
        { (ushort)((2 << 8) + (0 << 6) + (0 << 4) + (2 << 2) + 0), 10 },        // 2_0_0_2_0
        { (ushort)((0 << 8) + (0 << 6) + (2 << 4) + (0 << 2) + 2), 10 },        // 0_0_2_0_2
        { (ushort)((0 << 8) + (2 << 6) + (0 << 4) + (2 << 2) + 0), 10 },        // 0_2_0_2_0
        { (ushort)((0 << 8) + (2 << 6) + (0 << 4) + (0 << 2) + 2), 10 },        // 0_2_0_0_2

        // Def
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 2), 19000000 },     // 1_1_1_1_2
        { (ushort)((2 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 1), 19000000 },     // 2_1_1_1_1
        { (ushort)((1 << 8) + (2 << 6) + (1 << 4) + (1 << 2) + 1), 19000000 },     // 1_2_1_1_1
        { (ushort)((1 << 8) + (1 << 6) + (2 << 4) + (1 << 2) + 1), 19000000 },     // 1_1_2_1_1
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (2 << 2) + 1), 19000000 },     // 1_1_1_2_1
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (2 << 2) + 0), 3550000 },      // 1_1_1_2_0
        { (ushort)((0 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 2), 3550000 },      // 0_1_1_1_2
        { (ushort)((0 << 8) + (2 << 6) + (1 << 4) + (1 << 2) + 1), 3550000 },      // 0_2_1_1_1
        { (ushort)((2 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 0), 3550000 },      // 2_1_1_1_0
        { (ushort)((1 << 8) + (2 << 6) + (1 << 4) + (1 << 2) + 0), 3550000 },      // 1_2_1_1_0
        { (ushort)((1 << 8) + (1 << 6) + (2 << 4) + (1 << 2) + 0), 3550000 },      // 1_1_2_1_0
        { (ushort)((0 << 8) + (1 << 6) + (1 << 4) + (2 << 2) + 1), 3550000 },      // 0_1_1_2_1
        { (ushort)((0 << 8) + (1 << 6) + (2 << 4) + (1 << 2) + 1), 3550000 },      // 0_1_2_1_1
    };

    public static readonly Dictionary<ushort, int> PatternsPlayerOne = new Dictionary<ushort, int>()
    {
        // Off
        // 5 pions alignés
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 1), DEFEAT },    // 2_2_2_2_2

        // 4 pions alignés
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 0), -3999999 },      // 2_2_2_2_0
        { (ushort)((0 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 1), -3999999 },      // 0_2_2_2_2

        // 3 pions alignés
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (0 << 2) + 0), -520000 },
        { (ushort)((0 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 0), -520000 },
        { (ushort)((0 << 8) + (0 << 6) + (1 << 4) + (1 << 2) + 1), -520000 },

        // pions seul
        { (ushort)((1 << 8) + (0 << 6) + (0 << 4) + (0 << 2) + 0), -1 },
        { (ushort)((0 << 8) + (1 << 6) + (0 << 4) + (0 << 2) + 0), -1 },
        { (ushort)((0 << 8) + (0 << 6) + (1 << 4) + (0 << 2) + 0), -1 },
        { (ushort)((0 << 8) + (0 << 6) + (0 << 4) + (1 << 2) + 0), -1 },
        { (ushort)((0 << 8) + (0 << 6) + (0 << 4) + (0 << 2) + 1), -1 },

        // 4 pions séparés
        { (ushort)((1 << 8) + (0 << 6) + (1 << 4) + (1 << 2) + 1), -2899999 },
        { (ushort)((1 << 8) + (1 << 6) + (0 << 4) + (1 << 2) + 1), -2899999 },
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (0 << 2) + 1), -2899999 },

        // 3 pions séparés
        { (ushort)((1 << 8) + (0 << 6) + (1 << 4) + (0 << 2) + 1), -449000 },
        { (ushort)((1 << 8) + (0 << 6) + (1 << 4) + (1 << 2) + 0), -449000 },
        { (ushort)((0 << 8) + (1 << 6) + (0 << 4) + (1 << 2) + 1), -449000 },
        { (ushort)((1 << 8) + (0 << 6) + (0 << 4) + (1 << 2) + 1), -449000 },
        { (ushort)((1 << 8) + (1 << 6) + (0 << 4) + (0 << 2) + 1), -449000 },
        { (ushort)((0 << 8) + (1 << 6) + (1 << 4) + (0 << 2) + 1), -449000 },
        { (ushort)((1 << 8) + (1 << 6) + (0 << 4) + (1 << 2) + 0), -449000 }, 
        
        // 2 pions alignés
        { (ushort)((1 << 8) + (1 << 6) + (0 << 4) + (0 << 2) + 0), -15 },        // 2_2_0_0_0
        { (ushort)((0 << 8) + (1 << 6) + (1 << 4) + (0 << 2) + 0), -15 },        // 0_2_2_0_0
        { (ushort)((0 << 8) + (0 << 6) + (1 << 4) + (1 << 2) + 0), -15 },        // 0_0_2_2_2
        { (ushort)((0 << 8) + (0 << 6) + (0 << 4) + (1 << 2) + 1), -15 },        // 0_0_0_2_2
        // pions séparés, 2 pions séparés
        { (ushort)((1 << 8) + (0 << 6) + (1 << 4) + (0 << 2) + 0), -10 },        // 2_0_2_0_0
        { (ushort)((1 << 8) + (0 << 6) + (0 << 4) + (0 << 2) + 1), -10 },        // 2_0_0_0_2
        { (ushort)((1 << 8) + (0 << 6) + (0 << 4) + (1 << 2) + 0), -10 },        // 2_0_0_2_0
        { (ushort)((0 << 8) + (0 << 6) + (1 << 4) + (0 << 2) + 1), -10 },        // 0_0_2_0_2
        { (ushort)((0 << 8) + (1 << 6) + (0 << 4) + (1 << 2) + 0), -10 },        // 0_2_0_2_0
        { (ushort)((0 << 8) + (1 << 6) + (0 << 4) + (0 << 2) + 1), -10 },        // 0_2_0_0_2

        // Off capture, doit être multiplié ensuite selon le nombre de captures deja effectuées
        { (ushort)((1 << 8) + (2 << 6) + (2 << 4) + (1 << 2) + 1), -42 },
        { (ushort)((1 << 8) + (1 << 6) + (2 << 4) + (2 << 2) + 1), -42 },
        { (ushort)((1 << 8) + (2 << 6) + (2 << 4) + (1 << 2) + 0), -42 },
        { (ushort)((0 << 8) + (1 << 6) + (2 << 4) + (2 << 2) + 1), -42 },

        // Def
        { (ushort)((2 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 1), -8899999 },     // 1_1_1_1_2
        { (ushort)((1 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 2), -8899999 },     // 2_1_1_1_1
        { (ushort)((2 << 8) + (1 << 6) + (2 << 4) + (2 << 2) + 2), -8899999 },     // 1_2_1_1_1
        { (ushort)((2 << 8) + (2 << 6) + (1 << 4) + (2 << 2) + 2), -8899999 },     // 1_1_2_1_1
        { (ushort)((2 << 8) + (2 << 6) + (2 << 4) + (1 << 2) + 2), -8899999 },     // 1_1_1_2_1
        { (ushort)((2 << 8) + (2 << 6) + (2 << 4) + (1 << 2) + 0), -3549999 },      // 1_1_1_2_0
        { (ushort)((0 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 1), -3549999 },      // 0_1_1_1_2
        { (ushort)((0 << 8) + (1 << 6) + (2 << 4) + (2 << 2) + 2), -3549999 },      // 0_2_1_1_1
        { (ushort)((1 << 8) + (2 << 6) + (2 << 4) + (2 << 2) + 0), -3549999 },      // 2_1_1_1_0
        { (ushort)((2 << 8) + (1 << 6) + (2 << 4) + (2 << 2) + 0), -3549999 },      // 1_2_1_1_0
        { (ushort)((2 << 8) + (2 << 6) + (1 << 4) + (2 << 2) + 0), -3549999 },      // 1_1_2_1_0
        { (ushort)((0 << 8) + (2 << 6) + (2 << 4) + (1 << 2) + 2), -3549999 },      // 0_1_1_2_1
        { (ushort)((0 << 8) + (2 << 6) + (1 << 4) + (2 << 2) + 2), -3549999 },      // 0_1_2_1_1

        // Def capture
        { (ushort)((1 << 8) + (1 << 6) + (1 << 4) + (2 << 2) + 0), 42 },
        { (ushort)((0 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 2), 42 },
        { (ushort)((2 << 8) + (1 << 6) + (1 << 4) + (1 << 2) + 0), 42 },
        { (ushort)((0 << 8) + (2 << 6) + (1 << 4) + (1 << 2) + 1), 42 },
    };


    public static readonly Dictionary<int, Dictionary<ushort, int>> Patterns = new Dictionary<int, Dictionary<ushort, int>>()
    {
        { 1, PatternsPlayerOne },
        { 2, PatternsIA },
    };
}
