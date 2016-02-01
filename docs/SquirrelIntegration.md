# Squirrel Application Integration


Just add this code to your .NET application.

Better when the application is fully loaded, in order to not slow down application startup time.


```
        Task.Run(async () =>
          {
              using (var mgr = new UpdateManager(@"https://s3-<region>.amazonaws.com/<appbucket>"))              
              {
                  try
                  {
                      await mgr.UpdateApp();
                  }
                  catch (Exception ex)
                  {

                  }
              }
          });

```
## Reference

- [Squirrel Github integration](https://github.com/Squirrel/Squirrel.Windows/blob/master/docs/getting-started/1-integrating.md)
