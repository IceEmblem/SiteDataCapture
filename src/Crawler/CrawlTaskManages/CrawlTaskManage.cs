using Crawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.CrawlTaskManages
{
    public class CrawlTaskManage
    {
        private int _currentThreadCount { get; set; }

        private int _maxThreadCount { get; set; }

        private int _crawlDepth { get; set; }

        private Queue<ICrawlTask> _taskQueue { get; set; } = new Queue<ICrawlTask>();

        private HashSet<string> _accessedUrls { get; set; } = new HashSet<string>();

        private object _taskQueueLock { get; } = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxThreadCount">最大线程数</param>
        /// <param name="crawlDepth">最大抓取深度，如果小于或等于0，则为255</param>
        public CrawlTaskManage(int MaxThreadCount, int crawlDepth)
        {
            _maxThreadCount = MaxThreadCount;

            _crawlDepth = crawlDepth;
        }

        /// <summary>
        /// 启动新任务
        /// </summary>
        public void AddNewTask(ICrawlTask task)
        {
            _taskQueue.Enqueue(task);
        } 

        public void Start()
        {
            StartNewTask();

            while (_currentThreadCount > 0)
            {
                if (_currentThreadCount < _maxThreadCount && _taskQueue.Count > 0)
                {
                    StartNewTask();
                }
                else
                {
                    Thread.Sleep(2000);
                }
            }
        }

        private void EndTask() {
            _currentThreadCount--;
        }

        private void StartNewTask() 
        {
            _currentThreadCount++;
            ExecTask();
        }

        private void ExecTask() {
            try
            {
                ICrawlTask task = null;

                lock (_taskQueueLock)
                {
                    if (_taskQueue.Count == 0)
                    {
                        EndTask();
                        return;
                    }

                    task = _taskQueue.Dequeue();
                }

                Task.Run(() => {
                    try
                    {
                        return task.Run();
                    }
                    catch (Exception) 
                    {
                        return new ICrawlTask[] { };
                    }
                }).ContinueWith((Task<IEnumerable<ICrawlTask>> tasks) => {
                    try
                    {
                        foreach (var t in tasks.Result)
                        {
                            if ((_crawlDepth > 0 ? t.Depth <= _crawlDepth : t.Depth <= 255) &&
                                !_accessedUrls.Contains(t.Url)
                            )
                            {
                                _accessedUrls.Add(t.Url);
                                _taskQueue.Enqueue(t);
                            }
                        }
                    }
                    catch
                    {
                        EndTask();
                        return;
                    }

                    ExecTask();
                });
            }
            catch (Exception)
            {
                EndTask();
                return;
            }
        }
    }
}
