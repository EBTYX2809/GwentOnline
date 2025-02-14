using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Gwent_Release.Models
{
    public static class MusicBG
    {
        private static List<string> PlayList = new List<string>
        {      
            // application
            "pack://siteoforigin:,,,/BGMusic/A Story You Won't Believe.mp3",
            "pack://siteoforigin:,,,/BGMusic/Back on Path.mp3",
            "pack://siteoforigin:,,,/BGMusic/Unreleased Gwent Track.mp3"
        };

        private static int currentTrack = 0;
        private static MediaElement bgMusic;

        public static void Initialize(MediaElement musicElement)
        {
            bgMusic = musicElement;
            bgMusic.MediaEnded += (sender, e) => PlayNextTrack();
        }

        public static void PlayNextTrack()
        {
            if (currentTrack >= PlayList.Count) currentTrack = 0;
            bgMusic.Source = new Uri(PlayList[currentTrack], UriKind.Absolute);
            bgMusic.Play();
            currentTrack++;
        }

        public static void SetVolume(double volume) 
        { 
            if (bgMusic != null) bgMusic.Volume = volume; 
        }
    }
}
