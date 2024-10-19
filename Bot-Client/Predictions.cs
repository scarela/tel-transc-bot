namespace Replicate
{
    public class Prediction
    {
        public string Id { get; set; }
        public string model { get; set; }
        public string version { get; set; }
        public PredictionInput input { get; set; }
        public object output { get; set; }
        public string logs { get; set; }
        public object error { get; set; }
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
}