using System.Collections.Generic;
using UnityEngine;

namespace Beatmap
{
    // ============================ INFO ====================================
        
    [System.Serializable]
    public class SongInfo
    {
        public AudioInfo audio;
    }
    [System.Serializable]
    public class AudioInfo
    {
        public string audioDataFilename;
        public float bpm;
    }
    
    
    // ============================ BEATMAP DATA ====================================
    [System.Serializable]
    public class ColorNote : IBeatmapObject
    {
        public float beat; // Beat
        public int x;
        public int y;
        public int d; // Direction

        public float Beat => beat;
    }

    public interface IBeatmapObject
    {
        public float Beat { get; }
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

    public class BeatmapData
    {
        public List<ColorNote> colorNotes;
        public List<BombNote> bombNotes;
        public List<Obstacle> obstacles;

        public BeatmapData()
        {
            colorNotes = new List<ColorNote>();
            bombNotes = new List<BombNote>();
            obstacles = new List<Obstacle>();
        }
    }

    public class BeatmapdataParser
    {
        public static AudioInfo ParseBeatmapInfo(string jsonString)
        {
            // Parse the JSON string
            SongInfo songInfo = JsonUtility.FromJson<SongInfo>(jsonString);
            // Access the desired fields
            string audioDataFilename = songInfo.audio.audioDataFilename;
            float bpm = songInfo.audio.bpm;
            
            return songInfo.audio;
        }
        
        public static BeatmapData ParseBeatmapData(string jsonString)
        {
            RootObject data = JsonUtility.FromJson<RootObject>(jsonString);
            BeatmapData beatmapData = new BeatmapData();

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
                    beat = note.b,
                    x = noteData.x,
                    y = noteData.y,
                    d = noteData.d
                };

                beatmapData.colorNotes.Add(combinedNote);
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

                beatmapData.bombNotes.Add(combinedNote);
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

                beatmapData.obstacles.Add(combinedObstacle);
            }

            return beatmapData;
        }
    }
}