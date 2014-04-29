using System;
using Android.App;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.OS;
using Android.Views;
using Android.Graphics;
using Android.Content;
using Android.Util;
using System.Threading.Tasks;
using Android.Media;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;

namespace AnimatedGif
{
    [Activity(Label = "AnimatedGif", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private AnimationView animate;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var btnDog = FindViewById<Button>(Resource.Id.btnDog);
            var btnCat = FindViewById<Button>(Resource.Id.btnCat);
            animate = FindViewById<AnimationView>(Resource.Id.imgAnimate);
            btnDog.Click += delegate
            {
                AnimateAnimal(true);
            };
            btnCat.Click += delegate
            {
                AnimateAnimal();
            };
        }

        private async void AnimateAnimal(bool isDog = false)
        {
            var animal = isDog ? Resource.Drawable.dog : Resource.Drawable.cat;
            var stream = Resources.OpenRawResource(animal);
            await animate.Initialise(stream);
        }
    }

    public class AnimationView : View
    {
        private Movie movie;
        private long movieStart;
        private bool playing = true;

        public AnimationView(Context context, IAttributeSet attr) : base(context, attr)
        {
        }

        public AnimationView(Context context, IAttributeSet attr, int defStyle) : base(context, attr, defStyle)
        {
        }

        public async static Task<byte[]> ReadAllIn(System.IO.Stream input)
        {
            return await Task.Run(() =>
            {
                var buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                        ms.Write(buffer, 0, read);
                    return ms.ToArray();
                }
            });
        }

        public async Task Initialise(System.IO.Stream input)
        {
            Focusable = true;
            try
            {
                if (false)
                {
                    movie = Movie.DecodeStream(input);
                    movieStart = 0;
                }
                else
                {
                    var array = await ReadAllIn(input);
                    movie = Movie.DecodeByteArray(array, 0, array.Length);
                    var duration = movie.Duration();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown in Initialise - {0} -- {1}", ex.Message, ex.StackTrace);
            }
        }

        public void Start()
        {
            playing = true;
            Invalidate();
        }

        public void Stop()
        {
            playing = false;
        }

        protected override void OnDraw(Canvas canvas)
        {
            canvas.DrawColor(Color.Transparent);
            var p = new Paint()
            {
                AntiAlias = true
            };
            SetLayerType(LayerType.Software, p);
            var now = SystemClock.UptimeMillis();
            if (movieStart == 0)
                movieStart = now;
            if (movie != null)
            {
                int dur = movie.Duration();
                if (dur == 0)
                    dur = 1000;
                var relTime = (int)((now - movieStart) % dur);
                movie.SetTime(relTime);
                var movieWidth = (float)movie.Width();
                var movieHeight = (float)movie.Height();
                var scale = 1.0f;
                if (movieWidth > movieHeight)
                {
                    scale = Width / movieWidth;
                    if (scale * movieHeight > Height)
                        scale = Height / movieHeight;
                }
                else
                {
                    scale = Height / movieHeight;
                    if (scale * movieWidth > Width)
                        scale = Height / movieWidth;
                }
                canvas.Scale(scale, scale);
                try
                {
                    movie.Draw(canvas, 0, 0, p);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception thrown in OnDraw : {0}--{1}", ex.Message, ex.StackTrace);
                }

                if (playing)
                    Invalidate();
            }
        }
    }
}


