using System.Collections.Generic;
using UnityEngine;

namespace BeatmapData
{
    [System.Serializable]
    public class ColorNote
    {
        public float b; // Beat
        public int x;
        public int y;
        public int d; // Direction
    }

    [System.Serializable]
    public class BombNote
    {
        public float b; // Beat
        public int x;
        public int y;
    }

    [System.Serializable]
    public class Obstacle
    {
        public float b; // Beat
        public int x;
        public int y;
        public float d; // Depth
        public int w;   // Width
        public int h;   // Height
    }

    [System.Serializable]
    public class ColorNoteJson
    {
        public float b;
        public int i;
    }

    [System.Serializable]
    public class BombNoteJson
    {
        public float b;
        public int i;
    }

    [System.Serializable]
    public class ObstacleJson
    {
        public float b;
        public int i;
    }

    [System.Serializable]
    public class ColorNoteDataJson
    {
        public int x;
        public int y;
        public int d;
    }

    [System.Serializable]
    public class BombNoteDataJson
    {
        public int x;
        public int y;
    }

    [System.Serializable]
    public class ObstacleDataJson
    {
        public int x;
        public int y;
        public float d;
        public int w;
        public int h;
    }

    [System.Serializable]
    public class RootObject
    {
        public List<ColorNoteJson> colorNotes;
        public List<ColorNoteDataJson> colorNotesData;
        public List<BombNoteJson> bombNotes;
        public List<BombNoteDataJson> bombNotesData;
        public List<ObstacleJson> obstacles;
        public List<ObstacleDataJson> obstaclesData;
    }

    public class Beatmap
    {
        public List<ColorNote> colorNotes;
        public List<BombNote> bombNotes;
        public List<Obstacle> obstacles;

        public Beatmap()
        {
            colorNotes = new List<ColorNote>();
            bombNotes = new List<BombNote>();
            obstacles = new List<Obstacle>();
        }
    }

    public class JsonParser
    {
        public static Beatmap ParseBeatmapData(string jsonString)
        {
            RootObject data = JsonUtility.FromJson<RootObject>(jsonString);
            Beatmap beatmap = new Beatmap();

            // Parse color notes
            foreach (var note in data.colorNotes)
            {
                ColorNoteDataJson noteData = new ColorNoteDataJson();
                if (note.i < data.colorNotesData.Count)
                {
                    noteData = data.colorNotesData[note.i];
                }

                ColorNote combinedNote = new ColorNote
                {
                    b = note.b,
                    x = noteData.x,
                    y = noteData.y,
                    d = noteData.d
                };

                beatmap.colorNotes.Add(combinedNote);
            }

            // Parse bomb notes
            foreach (var note in data.bombNotes)
            {
                BombNoteDataJson noteData = new BombNoteDataJson();
                if (note.i < data.bombNotesData.Count)
                {
                    noteData = data.bombNotesData[note.i];
                }

                BombNote combinedNote = new BombNote
                {
                    b = note.b,
                    x = noteData.x,
                    y = noteData.y
                };

                beatmap.bombNotes.Add(combinedNote);
            }

            // Parse obstacles
            foreach (var obstacle in data.obstacles)
            {
                ObstacleDataJson obstacleData = new ObstacleDataJson();
                if (obstacle.i < data.obstaclesData.Count)
                {
                    obstacleData = data.obstaclesData[obstacle.i];
                }

                Obstacle combinedObstacle = new Obstacle
                {
                    b = obstacle.b,
                    x = obstacleData.x,
                    y = obstacleData.y,
                    d = obstacleData.d,
                    w = obstacleData.w,
                    h = obstacleData.h
                };

                beatmap.obstacles.Add(combinedObstacle);
            }

            return beatmap;
        }
    }
}