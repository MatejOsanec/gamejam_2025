using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AlphabetScrollInfo {

    public class Data {
        public readonly char character;
        public readonly int cellIdx;

        public Data(char character, int cellIdx) {
            this.character = character;
            this.cellIdx = cellIdx;
        }
    }
}