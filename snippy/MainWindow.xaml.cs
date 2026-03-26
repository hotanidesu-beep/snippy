using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace snippy
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private int _points = 0;
        private Random _random = new Random();
        private readonly string _saveFilePath;
        
        public ObservableCollection<TodoItem> Todos { get; set; } = new ObservableCollection<TodoItem>();

        // 5x7 Pixel Art Definitions (1 = filled, 0 = empty)
        private readonly Dictionary<char, int[]> _pixelFont = new Dictionary<char, int[]>
        {
            {'0', new int[] { 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1 }},
            {'1', new int[] { 0,0,1,0,0, 0,1,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,1,1,1,0 }},
            {'2', new int[] { 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 1,1,1,1,1 }},
            {'3', new int[] { 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1 }},
            {'4', new int[] { 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 0,0,0,0,1 }},
            {'5', new int[] { 1,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1 }},
            {'6', new int[] { 1,1,1,1,1, 1,0,0,0,0, 1,0,0,0,0, 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1 }},
            {'7', new int[] { 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 0,0,0,1,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0 }},
            {'8', new int[] { 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1 }},
            {'9', new int[] { 1,1,1,1,1, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,1, 0,0,0,0,1, 0,0,0,0,1, 1,1,1,1,1 }},
            {':', new int[] { 0,0,0,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,0,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,0,0,0 }},
            {'/', new int[] { 0,0,0,0,1, 0,0,0,0,1, 0,0,0,1,0, 0,0,1,0,0, 0,1,0,0,0, 1,0,0,0,0, 1,0,0,0,0 }},
            {' ', new int[] { 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0, 0,0,0,0,0 }},
            {'P', new int[] { 1,1,1,1,0, 1,0,0,0,1, 1,0,0,0,1, 1,1,1,1,0, 1,0,0,0,0, 1,0,0,0,0, 1,0,0,0,0 }},
            {'o', new int[] { 0,0,0,0,0, 0,0,0,0,0, 0,1,1,1,0, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 0,1,1,1,0 }},
            {'i', new int[] { 0,0,1,0,0, 0,0,0,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0 }},
            {'n', new int[] { 0,0,0,0,0, 0,0,0,0,0, 1,1,1,1,0, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1, 1,0,0,0,1 }},
            {'t', new int[] { 0,0,1,0,0, 0,1,1,1,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,1,0,0, 0,0,0,1,1 }},
            {'s', new int[] { 0,0,0,0,0, 0,0,0,0,0, 0,1,1,1,1, 1,0,0,0,0, 0,1,1,1,0, 0,0,0,0,1, 1,1,1,1,0 }}
        };

        private double _lastKnobAngle = 0;
        private double _accumulatedKnobAngle = 0;
        
        // ラックチェック State
        private int _slotDigit1 = 0;
        private int _slotDigit2 = 0;
        private int _slotDigit3 = 0;
        private bool _isSpinningSlot = false;
        private DispatcherTimer _slotSpinTimer;
        private DateTime _slotSpinStartTime;

        // Sticky Note Dragging State
        private bool _isDraggingNote = false;
        private Border _draggingNote = null;
        private Point _dragStartMousePos;
        private Point _dragStartNotePos;

        // Lucky Mode State
        private bool _isLuckyMode = false;
        private int _luckyRemainingSpins = 0;
        
        // Pity/Ceiling System
        private int _totalSpinsCounter = 0;

        // Customization State
        private string _tvFrameColor = "#333333";
        private string _screenGridColor = "#FF0D3A20";
        private double _tvOuterRadius = 8.0;
        private double _tvInnerRadius = 8.0;
        private int _plantLevel = 0;

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up ラックチェック timer
            _slotSpinTimer = new DispatcherTimer();
            _slotSpinTimer.Interval = TimeSpan.FromMilliseconds(50); // fast update for "spinning" effect
            _slotSpinTimer.Tick += SlotSpinTimer_Tick;
            
            // Set up save file path in MyDocuments
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appFolderPath = System.IO.Path.Combine(docPath, "SnippyClock");
            if (!Directory.Exists(appFolderPath))
                Directory.CreateDirectory(appFolderPath);
            _saveFilePath = System.IO.Path.Combine(appFolderPath, "save.json");

            lstTodos.ItemsSource = Todos;

            // Start clock
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Initial update
            Timer_Tick(null, null);

            // Setup Knob Event
            Knob1.AngleChangedEvent += Knob1_AngleChanged;

            // Start CRT Scanline Animation
            this.Loaded += (s, e) => {
                var anim = (Storyboard)this.Resources["ScanlineAnim"];
                anim.Begin();
                LoadData(); // Load saved points and sticky notes
                UpdatePointsDisplay();
                UpdateSlotDisplay(); // Initial ラックチェック state
                ApplyCustomizations(); // Apply loaded look
                // ApplyPlantLevelChanges(); // Apply plant and effects on load (Temporarily disabled)
            };
        }

        private void Knob1_AngleChanged(object sender, double newAngle)
        {
            if (_isSpinningSlot)
            {
                // Ignore knob while ラックチェック is spinning
                _lastKnobAngle = newAngle;
                return;
            }

            // Calculate angle delta
            double delta = newAngle - _lastKnobAngle;
            
            // Handle angle wrap-around (e.g. 350 to 10 -> delta = -340, actual should be +20)
            if (delta > 180) delta -= 360;
            else if (delta < -180) delta += 360;

            _lastKnobAngle = newAngle;
            _accumulatedKnobAngle += Math.Abs(delta);

            if (_accumulatedKnobAngle >= 360)
            {
                _accumulatedKnobAngle -= 360;
                StartSlotSpin();
            }
        }

        private void StartSlotSpin()
        {
            if (_points >= 10)
            {
                _isSpinningSlot = true;
                _slotSpinStartTime = DateTime.Now;
                _slotSpinTimer.Start();
            }
            else
            {
                // Not enough points, reset accumulated angle to avoid infinite loop of trying
                _accumulatedKnobAngle = 0;
            }
        }

        private void SlotSpinTimer_Tick(object sender, EventArgs e)
        {
            // Randomize digits while spinning
            _slotDigit1 = _random.Next(0, 10);
            _slotDigit2 = _random.Next(0, 10);
            _slotDigit3 = _random.Next(0, 10);
            UpdateSlotDisplay(true);

            // Check if 2 seconds have passed
            if ((DateTime.Now - _slotSpinStartTime).TotalSeconds >= 2.0)
            {
                _slotSpinTimer.Stop();
                FinishSlotSpin();
            }
        }

        private void FinishSlotSpin()
        {
            int cost = 10;
            _totalSpinsCounter++; // 回数をカウントアップ
            
            // Finalize ラックチェック numbers
            _slotDigit1 = _random.Next(0, 10);
            _slotDigit2 = _random.Next(0, 10);
            _slotDigit3 = _random.Next(0, 10);

            // 100回転ごとの高確率（天井）システム（例: 90%で揃う）
            if (_totalSpinsCounter > 0 && _totalSpinsCounter % 100 == 0)
            {
                if (_random.NextDouble() < 0.90) // 90% chance
                {
                    _slotDigit1 = _random.Next(0, 10);
                    _slotDigit2 = _slotDigit1;
                    _slotDigit3 = _slotDigit1;
                }
            }
            // 幸運中の場合は高確率で当たりにする
            else if (_isLuckyMode)
            {
                // 例えば30%の確率で強制的に揃える
                if (_random.NextDouble() < 0.30)
                {
                    _slotDigit1 = _random.Next(0, 10);
                    _slotDigit2 = _slotDigit1;
                    _slotDigit3 = _slotDigit1;
                }
            }

            bool isWin = false;
            int bonus = 0;

            // Check ラックチェック win condition
            if (_slotDigit1 == _slotDigit2 && _slotDigit2 == _slotDigit3)
            {
                isWin = true;
                if (_slotDigit1 == 7)
                {
                    // Jackpot 777: No cost, big bonus
                    cost = 0;
                    bonus = 100;
                }
                else
                {
                    // Other matched digits: No cost, small bonus
                    cost = 0;
                    bonus = 20;
                }

                // 幸運中の突入判定（通常時に当たりが出たら突入）
                if (!_isLuckyMode)
                {
                    _isLuckyMode = true;
                    _luckyRemainingSpins = 10;
                }
                // （幸運中に当たりが出ても _luckyRemainingSpins は増えない）
            }

            // 幸運中なら残り回数を減らす
            if (_isLuckyMode && !isWin || (_isLuckyMode && isWin && _luckyRemainingSpins > 0))
            {
                _luckyRemainingSpins--;
                
                // 10回終了時の継続判定
                if (_luckyRemainingSpins <= 0)
                {
                    // 一定確率（例: 30%）でさらに10回延長
                    if (_random.NextDouble() < 0.30)
                    {
                        _luckyRemainingSpins = 10; // 継続
                        PlayLuckyContinueAnimation();
                    }
                    else
                    {
                        _isLuckyMode = false; // 終了
                    }
                }
            }

            _points -= cost;
            if (bonus > 0)
            {
                AddPoints(bonus);
            }
            
            UpdatePointsDisplay();
            UpdateSlotDisplay(false, isWin); // Fixed state

            _plantLevel++;
            
            if (isWin)
            {
                PlayWinAnimation();
            }
            else
            {
                ApplyRandomMinorChange();
                PlayFeedbackAnimation();
            }

            SaveData();

            // Unlock knob
            _isSpinningSlot = false;
        }

        private void UpdateSlotDisplay(bool isSpinning = false, bool isWin = false)
        {
            if (pixelCanvasSlot != null)
            {
                string slotText = $"{_slotDigit1} {_slotDigit2} {_slotDigit3}";
                Color color = Colors.White;
                
                if (!isSpinning)
                {
                    color = isWin ? Colors.Gold : Color.FromArgb(255, 200, 200, 200);
                }

                DrawPixelText(pixelCanvasSlot, slotText, 1, new SolidColorBrush(color));

                if (pixelCanvasSlot.Effect is DropShadowEffect shadow)
                {
                    shadow.Color = isWin ? Colors.Gold : Colors.White;
                }
            }
        }

        private void PlayLuckyContinueAnimation()
        {
            // 幸運継続時の演出（画面が全体的に赤く光るなど）
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = Brushes.Red;
                    flashAnim.Begin();
                    
                    var resetTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    resetTimer.Tick += (s, e) => {
                        FlashOverlay.Fill = Brushes.White;
                        resetTimer.Stop();
                    };
                    resetTimer.Start();
                }

                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();
            }
            catch { }
        }

        private void PlayWinAnimation()
        {
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                if (flashAnim != null && FlashOverlay != null)
                {
                    FlashOverlay.Fill = Brushes.Gold;
                    flashAnim.Begin();
                    
                    var resetTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    resetTimer.Tick += (s, e) => {
                        FlashOverlay.Fill = Brushes.White;
                        resetTimer.Stop();
                    };
                    resetTimer.Start();
                }

                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();

                if (pixelCanvasSlot != null)
                {
                    var st = new ScaleTransform(1, 1);
                    pixelCanvasSlot.RenderTransform = st;
                    pixelCanvasSlot.RenderTransformOrigin = new Point(0.5, 0.5);
                    
                    var animX = new DoubleAnimation(1, 1.5, TimeSpan.FromMilliseconds(200)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3) };
                    var animY = new DoubleAnimation(1, 1.5, TimeSpan.FromMilliseconds(200)) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(3) };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
                }
            }
            catch { }
        }

        private void PlayFeedbackAnimation()
        {
            try
            {
                var flashAnim = (Storyboard)this.Resources["FlashAnim"];
                flashAnim?.Begin();

                var shakeAnim = (Storyboard)this.Resources["ShakeAnim"];
                shakeAnim?.Begin();
            }
            catch { }
        }

        private void ApplyRandomMinorChange()
        {
            int choice = _random.Next(4);
            switch(choice)
            {
                case 0: // Change TV Frame Color slightly
                    _tvFrameColor = TweakHexColor(_tvFrameColor);
                    break;
                case 1: // Change Screen Grid Color slightly
                    _screenGridColor = TweakHexColor(_screenGridColor);
                    break;
                case 2: // Change Outer Radius slightly
                    _tvOuterRadius = Math.Max(0, Math.Min(25, _tvOuterRadius + (_random.NextDouble() * 2 - 1))); // +/- 1
                    break;
                case 3: // Change Inner Radius slightly
                    _tvInnerRadius = Math.Max(0, Math.Min(15, _tvInnerRadius + (_random.NextDouble() * 2 - 1))); // +/- 1
                    break;
            }
            ApplyCustomizations();
        }

        private string TweakHexColor(string hex)
        {
            try
            {
                Color c = (Color)ColorConverter.ConvertFromString(hex);
                // Adjust RGB each by -10 to +10 randomly
                byte r = (byte)Math.Max(0, Math.Min(255, c.R + _random.Next(-10, 11)));
                byte g = (byte)Math.Max(0, Math.Min(255, c.G + _random.Next(-10, 11)));
                byte b = (byte)Math.Max(0, Math.Min(255, c.B + _random.Next(-10, 11)));
                return $"#{(c.A):X2}{r:X2}{g:X2}{b:X2}";
            }
            catch { return hex; }
        }

        private void ApplyCustomizations()
        {
            try
            {
                if (TvBodyPath != null)
                    TvBodyPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_tvFrameColor));
                
                if (ScreenGrid != null)
                    ScreenGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_screenGridColor));
                
                if (TvOuterBox != null)
                {
                    TvOuterBox.RadiusX = _tvOuterRadius;
                    TvOuterBox.RadiusY = _tvOuterRadius;
                }

                if (TvInnerBox != null)
                {
                    TvInnerBox.RadiusX = _tvInnerRadius;
                    TvInnerBox.RadiusY = _tvInnerRadius;
                }
            }
            catch { }
        }

        private void ApplyPlantLevelChanges()
        {
            /* Temporarily Disabled
            UpdatePlantDisplay();
            GenerateParticles();
            */
        }

        private void UpdatePlantDisplay()
        {
            if (pixelCanvasPlant != null)
            {
                DrawPlantPixelArt(_plantLevel);
            }
        }

        private void DrawPlantPixelArt(int level)
        {
            pixelCanvasPlant.Children.Clear();
            
            int height = Math.Min(5 + (level * 2), 30); // Max height
            
            // Stem
            Rectangle stem = new Rectangle
            {
                Width = 2,
                Height = height,
                Fill = new SolidColorBrush(Color.FromArgb(255, 34, 139, 34)) // ForestGreen
            };
            Canvas.SetLeft(stem, pixelCanvasPlant.Width / 2 - 1);
            Canvas.SetBottom(stem, 0);
            pixelCanvasPlant.Children.Add(stem);

            // Leaves
            int leafCount = Math.Min(level, 8);
            for(int i = 0; i < leafCount; i++)
            {
                Rectangle leaf = new Rectangle
                {
                    Width = 4 + (level/3.0),
                    Height = 2 + (level/5.0),
                    Fill = new SolidColorBrush(Color.FromArgb(255, 50, 205, 50)) // LimeGreen
                };
                Canvas.SetLeft(leaf, (pixelCanvasPlant.Width / 2) + (i % 2 == 0 ? 1 : - (4 + (level/3.0))));
                Canvas.SetBottom(leaf, 5 + (i * 4));
                pixelCanvasPlant.Children.Add(leaf);
            }
            
            // If level is high, add some "fruits" or flowers
            if (level >= 5)
            {
                int fruitCount = (level - 4);
                for(int i = 0; i < fruitCount; i++)
                {
                    Ellipse fruit = new Ellipse
                    {
                        Width = 4, Height = 4, Fill = Brushes.DeepPink
                    };
                    Canvas.SetLeft(fruit, (pixelCanvasPlant.Width / 2) + _random.Next(-10, 10));
                    Canvas.SetBottom(fruit, height - _random.Next(2, 15));
                    pixelCanvasPlant.Children.Add(fruit);
                }
            }
        }

        private void GenerateParticles()
        {
            if (EffectCanvas == null) return;
            
            EffectCanvas.Children.Clear();
            
            // Number of particles depends on plant level
            int particleCount = Math.Min(_plantLevel * 3, 50);
            
            for (int i = 0; i < particleCount; i++)
            {
                Ellipse particle = new Ellipse
                {
                    Width = _random.Next(2, 5),
                    Height = _random.Next(2, 5),
                    Fill = new SolidColorBrush(Color.FromArgb((byte)_random.Next(100, 200), 255, 255, 200)),
                    Opacity = _random.NextDouble()
                };

                Canvas.SetLeft(particle, _random.Next(0, (int)EffectCanvas.Width));
                Canvas.SetTop(particle, _random.Next(0, (int)EffectCanvas.Height));
                
                EffectCanvas.Children.Add(particle);

                // Add simple animation
                DoubleAnimation anim = new DoubleAnimation
                {
                    From = Canvas.GetTop(particle),
                    To = Canvas.GetTop(particle) - _random.Next(10, 30),
                    Duration = TimeSpan.FromSeconds(_random.Next(2, 5)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                particle.BeginAnimation(Canvas.TopProperty, anim);
                
                DoubleAnimation opacityAnim = new DoubleAnimation
                {
                    From = particle.Opacity,
                    To = 0.1,
                    Duration = TimeSpan.FromSeconds(_random.Next(1, 3)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    AutoReverse = true
                };
                particle.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            }
        }

        private void SaveData()
        {
            try
            {
                var notes = StickyNoteCanvas.Children.OfType<Border>()
                    .Where(b => b.Tag != null)
                    .Select(b => new SavedNote
                    {
                        Text = b.Tag.ToString(),
                        X = Canvas.GetLeft(b),
                        Y = Canvas.GetTop(b),
                        Angle = (b.RenderTransform as TransformGroup)?.Children.OfType<RotateTransform>().FirstOrDefault()?.Angle ?? 0,
                        Color = ((SolidColorBrush)b.Background).Color.ToString()
                    }).ToList();

                var data = new AppData
                {
                    Points = _points,
                    Notes = notes,
                    TvFrameColor = _tvFrameColor,
                    ScreenGridColor = _screenGridColor,
                    TvOuterRadius = _tvOuterRadius,
                    TvInnerRadius = _tvInnerRadius,
                    TotalSpins = _totalSpinsCounter
                };

                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception ex)
            {
                // Optionally handle save errors
            }
        }

        private void LoadData()
        {
            if (File.Exists(_saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveFilePath);
                    var data = JsonSerializer.Deserialize<AppData>(json);
                    
                    if (data != null)
                    {
                        _points = data.Points;
                        _tvFrameColor = data.TvFrameColor;
                        _screenGridColor = data.ScreenGridColor;
                        _tvOuterRadius = data.TvOuterRadius;
                        _tvInnerRadius = data.TvInnerRadius;
                        _plantLevel = data.PlantLevel;
                        _totalSpinsCounter = data.TotalSpins;
                        
                        Todos.Clear(); // 起動時にTodoリストをクリア
                        foreach (var noteData in data.Notes)
                        {
                            RestoreStickyNote(noteData);
                            // 保存されている付箋を元にTodoメニューにも追加
                            Todos.Add(new TodoItem { Text = noteData.Text, IsCompleted = false });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Optionally handle load errors
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            string timeStr = now.ToString("HH:mm:ss");
            string dateStr = now.ToString("yyyy/MM/dd");
            
            if (now.Second % 2 == 0)
                timeStr = now.ToString("HH mm ss");

            if (pixelCanvasTime != null) DrawPixelText(pixelCanvasTime, timeStr, 2, new SolidColorBrush(Color.FromArgb(255, 74, 255, 112))); // #FF4AFF70
            if (pixelCanvasDate != null) DrawPixelText(pixelCanvasDate, dateStr, 1, new SolidColorBrush(Color.FromArgb(255, 74, 255, 112)));
        }

        private void DrawPixelText(Canvas canvas, string text, int pixelSize, SolidColorBrush brush)
        {
            canvas.Children.Clear();
            
            int charWidth = 5;
            int charHeight = 7;
            int spacingX = 1;
            int currentX = 0;

            foreach (char c in text)
            {
                if (_pixelFont.ContainsKey(c))
                {
                    int[] pixels = _pixelFont[c];
                    for (int y = 0; y < charHeight; y++)
                    {
                        for (int x = 0; x < charWidth; x++)
                        {
                            int index = y * charWidth + x;
                            if (pixels[index] == 1)
                            {
                                Rectangle rect = new Rectangle
                                {
                                    Width = pixelSize,
                                    Height = pixelSize,
                                    Fill = brush
                                };
                                Canvas.SetLeft(rect, currentX + (x * pixelSize));
                                Canvas.SetTop(rect, y * pixelSize);
                                canvas.Children.Add(rect);
                            }
                        }
                    }
                }
                currentX += (charWidth + spacingX) * pixelSize;
            }
            canvas.Width = currentX;
            canvas.Height = charHeight * pixelSize;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Clear focus to allow any active TextBox to fire LostFocus and exit edit mode
            Keyboard.ClearFocus();

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnToggleTodo_Click(object sender, RoutedEventArgs e)
        {
            TodoAreaBorder.Visibility = TodoAreaBorder.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (TodoAreaBorder.Visibility == Visibility.Visible)
            {
                this.Height = 600; // Expand to show todos + offset
            }
            else
            {
                this.Height = 400; // Collapse (Base is 400)
            }
        }

        private void BtnAddTodo_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtTodoInput.Text))
            {
                string text = txtTodoInput.Text.Trim();

                // 隠しコマンド（デバッグ用）
                if (text.ToLower() == "god")
                {
                    AddPoints(1000);
                    SaveData();
                    txtTodoInput.Text = string.Empty;
                    return;
                }

                Todos.Add(new TodoItem { Text = text, IsCompleted = false });
                txtTodoInput.Text = string.Empty;
                AddStickyNote(text);
                SaveData();
            }
        }

        private void AddStickyNote(string text)
        {
            // Center of TV is around (150, 125) in Window coordinates
            // TV Frame bounds relative to Window approx: X: 55 to 245, Y: 55 to 195
            var note = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, (byte)_random.Next(200, 256), (byte)_random.Next(200, 256), (byte)_random.Next(150, 200))),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Width = 60,
                Height = 60,
                Padding = new Thickness(4),
                Tag = text // Store original text
            };

            var textBlock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Comic Sans MS"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            note.Child = textBlock;

            // Context Menu setup
            var contextMenu = new ContextMenu();
            
            var editMenuItem = new MenuItem { Header = "編集 (Edit)" };
            editMenuItem.Click += (s, e) => EditStickyNote(note, textBlock);
            
            var completeMenuItem = new MenuItem { Header = "達成 (Complete)" };
            completeMenuItem.Click += (s, e) => {
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(text);
                AddPoints(10);
                SaveData();
            };
            
            var deleteMenuItem = new MenuItem { Header = "削除 (Delete)" };
            deleteMenuItem.Click += (s, e) => {
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(text);
                SaveData();
            };

            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(completeMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            note.ContextMenu = contextMenu;

            // Add drop shadow
            note.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 3,
                ShadowDepth = 2,
                Opacity = 0.5
            };

            // The TV Container is at Margin 100,100 and size is 200x150.
            // TV Bounds in Window: X = 100 to 300, Y = 100 to 250
            // Window Size: Width 400, Height 400 (or 600)
            // Sticky Note Size: Width 60, Height 60
            // We want sticky notes to be completely outside the TV bounds and not overlapping each other.

            double x = 0;
            double y = 0;
            double angle = 0;
            bool positionFound = false;
            int maxAttempts = 50;
            
            // Get existing notes bounds to prevent overlap
            var existingNotesRects = StickyNoteCanvas.Children.OfType<Border>()
                .Select(b => new Rect(Canvas.GetLeft(b), Canvas.GetTop(b), b.Width, b.Height))
                .ToList();

            for (int i = 0; i < maxAttempts; i++)
            {
                int edge = _random.Next(4);
                if (edge == 0) // Top
                {
                    x = _random.Next(20, 320);
                    y = _random.Next(10, 40);
                    angle = _random.Next(-25, 25);
                }
                else if (edge == 1) // Right
                {
                    x = _random.Next(310, 330);
                    y = _random.Next(50, 250);
                    angle = _random.Next(15, 45);
                }
                else if (edge == 2) // Bottom
                {
                    x = _random.Next(20, 320);
                    y = _random.Next(260, 300);
                    angle = _random.Next(-25, 25);
                }
                else // Left
                {
                    x = _random.Next(10, 40);
                    y = _random.Next(50, 250);
                    angle = _random.Next(-45, -15);
                }

                // Check collision with existing notes
                Rect newRect = new Rect(x, y, 60, 60);
                
                // Allow a little bit of overlap (e.g., shrink the collision rect slightly)
                Rect collisionRect = new Rect(x + 5, y + 5, 50, 50);
                
                bool isOverlapping = existingNotesRects.Any(r => r.IntersectsWith(collisionRect));

                if (!isOverlapping)
                {
                    positionFound = true;
                    break;
                }
            }

            // If we couldn't find a totally clear spot after 50 attempts, 
            // just use the last generated position (it will overlap, but screen is full)

            var transform = new TransformGroup();
            transform.Children.Add(new RotateTransform(angle));
            note.RenderTransform = transform;
            note.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetLeft(note, x);
            Canvas.SetTop(note, y);
            
            // Re-enable hit test so user can click/right-click it
            note.IsHitTestVisible = true;
            
            // Register Drag and Drop events
            note.MouseLeftButtonDown += StickyNote_MouseLeftButtonDown;
            note.MouseMove += StickyNote_MouseMove;
            note.MouseLeftButtonUp += StickyNote_MouseLeftButtonUp;
            
            StickyNoteCanvas.Children.Add(note);
        }

        private void RestoreStickyNote(SavedNote noteData)
        {
            Color bgColor;
            try { bgColor = (Color)ColorConverter.ConvertFromString(noteData.Color); }
            catch { bgColor = Color.FromArgb(255, 250, 250, 200); }

            var note = new Border
            {
                Background = new SolidColorBrush(bgColor),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Width = 60,
                Height = 60,
                Padding = new Thickness(4),
                Tag = noteData.Text
            };

            var textBlock = new TextBlock
            {
                Text = noteData.Text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 10,
                Foreground = Brushes.Black,
                FontFamily = new FontFamily("Comic Sans MS"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            note.Child = textBlock;

            var contextMenu = new ContextMenu();
            var editMenuItem = new MenuItem { Header = "編集 (Edit)" };
            editMenuItem.Click += (s, e) => EditStickyNote(note, textBlock);
            
            var completeMenuItem = new MenuItem { Header = "達成 (Complete)" };
            completeMenuItem.Click += (s, e) => {
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(noteData.Text);
                AddPoints(10);
                SaveData();
            };
            
            var deleteMenuItem = new MenuItem { Header = "削除 (Delete)" };
            deleteMenuItem.Click += (s, e) => {
                StickyNoteCanvas.Children.Remove(note);
                RemoveTodoFromList(noteData.Text);
                SaveData();
            };

            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(completeMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            note.ContextMenu = contextMenu;

            note.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 3,
                ShadowDepth = 2,
                Opacity = 0.5
            };

            var transform = new TransformGroup();
            transform.Children.Add(new RotateTransform(noteData.Angle));
            note.RenderTransform = transform;
            note.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetLeft(note, noteData.X);
            Canvas.SetTop(note, noteData.Y);
            
            // Re-enable hit test so user can click/right-click it
            note.IsHitTestVisible = true;
            
            // Register Drag and Drop events
            note.MouseLeftButtonDown += StickyNote_MouseLeftButtonDown;
            note.MouseMove += StickyNote_MouseMove;
            note.MouseLeftButtonUp += StickyNote_MouseLeftButtonUp;
            
            StickyNoteCanvas.Children.Add(note);
        }

        private void StickyNote_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border note)
            {
                _isDraggingNote = true;
                _draggingNote = note;
                
                // Set the element to top ZIndex
                Panel.SetZIndex(note, StickyNoteCanvas.Children.Count);
                
                // Get starting position relative to the Canvas
                _dragStartMousePos = e.GetPosition(StickyNoteCanvas);
                _dragStartNotePos = new Point(Canvas.GetLeft(note), Canvas.GetTop(note));
                
                // Capture the mouse so it doesn't leave the element while dragging rapidly
                note.CaptureMouse();
                
                // We want to stop the main Window from dragging if we clicked a sticky note
                e.Handled = true;
            }
        }

        private void StickyNote_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingNote && _draggingNote != null && sender == _draggingNote)
            {
                // Calculate new position based on the delta from start
                Point currentMousePos = e.GetPosition(StickyNoteCanvas);
                
                double dx = currentMousePos.X - _dragStartMousePos.X;
                double dy = currentMousePos.Y - _dragStartMousePos.Y;
                
                double newLeft = _dragStartNotePos.X + dx;
                double newTop = _dragStartNotePos.Y + dy;
                
                // Restrict movement to within the StickyNoteCanvas (which matches window size)
                // Assuming note size is 60x60
                newLeft = Math.Max(0, Math.Min(StickyNoteCanvas.ActualWidth - _draggingNote.Width, newLeft));
                newTop = Math.Max(0, Math.Min(StickyNoteCanvas.ActualHeight - _draggingNote.Height, newTop));

                Canvas.SetLeft(_draggingNote, newLeft);
                Canvas.SetTop(_draggingNote, newTop);
            }
        }

        private void StickyNote_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingNote && _draggingNote != null && sender == _draggingNote)
            {
                _isDraggingNote = false;
                _draggingNote.ReleaseMouseCapture();
                _draggingNote = null;
                
                // Save positions after dragging finishes
                SaveData();
                e.Handled = true;
            }
        }

        private void EditStickyNote(Border note, TextBlock textBlock)
        {
            var textBox = new TextBox
            {
                Text = textBlock.Text,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true, // 複数行を許容するがEnter単押しで確定させたい
                Background = Brushes.White, // 視認性向上のため編集時は白背景
                Foreground = Brushes.Black,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Blue,
                FontSize = 10,
                FontFamily = new FontFamily("Comic Sans MS")
            };

            string originalText = textBlock.Text;
            bool isCommitted = false;

            RoutedEventHandler commitEdit = null;
            commitEdit = (s, e) =>
            {
                if (isCommitted) return;
                isCommitted = true;

                // イベントの解除
                textBox.LostFocus -= commitEdit;

                // 親(note)にフォーカスを戻す
                Keyboard.Focus(note);

                textBlock.Text = textBox.Text;
                note.Tag = textBox.Text;
                note.Child = textBlock; // 元のTextBlockに戻す
                UpdateTodoInList(originalText, textBox.Text);
                SaveData();
            };

            textBox.LostFocus += commitEdit;

            // PreviewKeyDown を使用して TextBox に消費される前に Enter と Esc を捕捉する
            textBox.PreviewKeyDown += (s, e) =>
            {
                // Shift+Enter は改行として許可し、Enter 単体は編集確定とする
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
                {
                    e.Handled = true; // TextBox の改行処理をキャンセル
                    commitEdit(null, null);
                }
                else if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    if (!isCommitted)
                    {
                        isCommitted = true;
                        textBox.LostFocus -= commitEdit;
                        Keyboard.Focus(note);
                        note.Child = textBlock; // 変更を破棄して戻す
                    }
                }
            };

            note.Child = textBox;
            
            // 確実なフォーカス移行のため少し遅延させる
            Dispatcher.BeginInvoke(new Action(() => {
                textBox.Focus();
                Keyboard.Focus(textBox);
                textBox.SelectAll();
            }), DispatcherPriority.Background);
        }

        private void RemoveTodoFromList(string text)
        {
            var itemToRemove = Todos.FirstOrDefault(t => t.Text == text);
            if (itemToRemove != null)
            {
                Todos.Remove(itemToRemove);
            }
        }

        private void UpdateTodoInList(string oldText, string newText)
        {
            var item = Todos.FirstOrDefault(t => t.Text == oldText);
            if (item != null)
            {
                item.Text = newText;
                // Force UI update for the list box item
                var index = Todos.IndexOf(item);
                Todos[index] = item;
            }
        }

        private void Todo_Checked(object sender, RoutedEventArgs e)
        {
            // Add points when checked
            AddPoints(10);
            
            var cb = sender as CheckBox;
            if (cb != null && cb.DataContext is TodoItem item)
            {
                // Remove item after a short delay
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    Todos.Remove(item);
                    
                    // Also remove the corresponding sticky note
                    var noteToRemove = StickyNoteCanvas.Children.OfType<Border>()
                                       .FirstOrDefault(b => b.Tag?.ToString() == item.Text);
                    if (noteToRemove != null)
                    {
                        StickyNoteCanvas.Children.Remove(noteToRemove);
                        SaveData();
                    }
                };
                timer.Start();
            }
        }

        private void Todo_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void AddPoints(int points)
        {
            _points += points;
            UpdatePointsDisplay();
            
            // Optional: small animation on time to indicate point gain
            if (pixelCanvasTime != null)
            {
                var st = new ScaleTransform(1, 1);
                pixelCanvasTime.RenderTransform = st;
                pixelCanvasTime.RenderTransformOrigin = new Point(0.5, 0.5);
                
                var animX = new DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(100)) { AutoReverse = true };
                var animY = new DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(100)) { AutoReverse = true };
                st.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
            }
        }

        private void UpdatePointsDisplay()
        {
            if (pixelCanvasPoints != null) 
                DrawPixelText(pixelCanvasPoints, $"{_points}", 1, new SolidColorBrush(Color.FromArgb(255, 255, 166, 64))); // #FFFFA640
        }
    }

    public class TodoItem
    {
        public string Text { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class AppData
    {
        public int Points { get; set; }
        public List<SavedNote> Notes { get; set; } = new List<SavedNote>();
        public string TvFrameColor { get; set; } = "#333333";
        public string ScreenGridColor { get; set; } = "#FF0D3A20";
        public double TvOuterRadius { get; set; } = 8.0;
        public double TvInnerRadius { get; set; } = 8.0;
        public int PlantLevel { get; set; } = 0;
        public int TotalSpins { get; set; } = 0;
    }

    public class SavedNote
    {
        public string Text { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Angle { get; set; }
        public string Color { get; set; }
    }
}
