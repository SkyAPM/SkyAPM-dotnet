namespace Sky.Apm.Sample.Logging
{
    public class Test
    {
        private readonly ILogger _logger;

        public Test(ILogger<Test> logger)
        {
            _logger = logger;
        }

        public void Create()
        {
            _logger.LogError("创建了一个用户对象",new List<string> { "asdasdas","3ewqeqdad","q34asdase2qq"});
        }
    }
}
