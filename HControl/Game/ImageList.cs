using System;
using System.Collections.Generic;
using System.IO;

namespace HControl.Game {
    public class ImageList {

        private readonly List<string> Images = [];
        private readonly Stack<string> PrevImages = [];
        private readonly Stack<string> NextImages = [];
        private string? CurrentImage = null;
        private static readonly string[] supportedExtensions = [".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp"];

        public ImageList() {}

        public void addImage(string img) {
            if (typeCheck(img) && !Images.Contains(img))
                Images.Add(img);
        }

        public void addFolder(string fName, bool includeSubfolders = false) {
            if (Directory.Exists(fName)) {
                foreach (var file in Directory.EnumerateFiles(fName)) {
                    if (typeCheck(file)) addImage(file);
                }
                if (includeSubfolders) {
                    foreach (var dir in Directory.EnumerateDirectories(fName)) 
                        addFolder(dir, true);
                }
            } else if (File.Exists(fName)) {
                addImage(fName);
            }
        }

        private static bool typeCheck(string fName) {
            return File.Exists(fName) && endsWith(fName, supportedExtensions);
        }

        private static bool endsWith(string fName, string[] ext) {
            foreach (var e in ext) if (fName.EndsWith(e)) return true;
            return false;
        }

        public string getNextImage() {
            string _result = "./Assets/img/intro_Lucas.png";
            if (NextImages.Count > 0) {
                _result = NextImages.Pop();
            } else if (Images.Count > 1) {
                _result = Images[Random.Shared.Next(Images.Count)];
                Images.Remove(_result);
            } else if (Images.Count == 1) {
                _result = Images[0];
                Images.Remove(_result);
                Images.AddRange(PrevImages);
                PrevImages.Clear();
            } else {
                return _result; // no Images to load, just return the default
            }
            if (CurrentImage != null) PrevImages.Push(CurrentImage);
            CurrentImage = _result;
            return _result;
        }

        public string? getPrevImage() {
            if (PrevImages.Count > 0) {
                string _result = PrevImages.Pop();
                if (CurrentImage != null) NextImages.Push(CurrentImage);
                CurrentImage = _result;
                return _result;
            } else {
                return null;
            }
        }

        public IEnumerable<string> getAllImages() {
            return Images;
        }

        public string this[int key] {
            get => Images[key];
        }


        public int ImageCount { get { return Images.Count; } } 
    }
}
