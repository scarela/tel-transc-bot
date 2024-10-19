namespace Replicate
{
    public class UnfinishedPredition : Prediction
    {
        public UnfinishedPredition()
        {
            output = null;
            error = null;
        }
    }

    public class CompletedPredition : Prediction
    {
        public new CompletedPredictionOutput output { get; set; }
    }

    public abstract class Prediction
    {
        public string id { get; set; }
        public string model { get; set; }
        public string version { get; set; }
        public PredictionInput input { get; set; }
        public object? output { get; set; }
        public string? logs { get; set; }
        public object? error { get; set; }
        public string status { get; set; }
        public string created_at { get; set; }
        public PredictionUrls urls { get; set; }
    }
    public class PredictionInput
    {
        public string text { get; set; }
    }

    public class PredictionUrls
    {
        public string cancel { get; set; }
        public string get { get; set; }
    }

    public class CompletedPredictionOutput
    {
        public string text { get; set; }
    }
}

/*
In progress or just created:
{
  "completed_at": null,
  "created_at": "2024-07-18T14:33:08.971000Z",
  "data_removed": false,
  "error": null,
  "id": "bz4jyw6wddrgg0cgrs1brkj480",
  "input": {
    "task": "transcribe",
    "audio": "https://api.telegram.org/file/bot___:AAHTJ3x9zgk_6yPTohn1KAp2LQClfh6lkDs/voice/file_0.oga",
    "language": "None",
    "timestamp": "chunk",
    "batch_size": 64,
    "diarise_audio": false
  },
  "logs": null,
  "metrics": {},
  "output": null,
  "started_at": "2024-07-18T14:33:08.983348Z",
  "status": "processing",
  "urls": {
    "get": "https://api.replicate.com/v1/predictions/bz4jyw6wddrgg0cgrs1brkj480",
    "cancel": "https://api.replicate.com/v1/predictions/bz4jyw6wddrgg0cgrs1brkj480/cancel"
  },
  "version": "3ab86df6c8f54c11309d4d1f930ac292bad43ace52d10c80d87eb258b3c9f79c"
}
*/

/*
Completed
{
  "completed_at": "2024-07-18T14:33:10.365456Z",
  "created_at": "2024-07-18T14:33:08.971000Z",
  "data_removed": false,
  "error": null,
  "id": "bz4jyw6wddrgg0cgrs1brkj480",
  "input": {
    "task": "transcribe",
    "audio": "https://api.telegram.org/file/bot___:AAHTJ3x9zgk_6yPTohn1KAp2LQClfh6lkDs/voice/file_0.oga",
    "language": "None",
    "timestamp": "chunk",
    "batch_size": 64,
    "diarise_audio": false
  },
  "logs": "Voila!✨ Your file has been transcribed!",
  "metrics": {
    "predict_time": 1.3821072810000001,
    "total_time": 1.394456
  },
  "output": {
    "text": " Chamo, mira, esas dos medias son de las que tú pusiste ahí para dar también.",
    "chunks": [
      {
        "text": " Chamo, mira, esas dos medias son de las que tú pusiste ahí para dar también.",
        "timestamp": [
          0,
          4.66
        ]
      }
    ]
  },
  "started_at": "2024-07-18T14:33:08.983348Z",
  "status": "succeeded",
  "urls": {
    "get": "https://api.replicate.com/v1/predictions/bz4jyw6wddrgg0cgrs1brkj480",
    "cancel": "https://api.replicate.com/v1/predictions/bz4jyw6wddrgg0cgrs1brkj480/cancel"
  },
  "version": "3ab86df6c8f54c11309d4d1f930ac292bad43ace52d10c80d87eb258b3c9f79c"
}
*/