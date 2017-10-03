using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace DriveFast
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D mCartemp;
        private Texture2D mCar1;
        private Texture2D mCar;
        private Texture2D mCar2;
        private Texture2D mBackground;
        private Texture2D mRoad;
        private Texture2D mHazard;
        private Texture2D mLifeBox; //医药包图片

        private KeyboardState mPreviousKeyboardState;
        private Vector2 mCarfirstPosition = new Vector2(100, 250);
        private Vector2 mCar1firstPosition = new Vector2(300, 250);
        private Vector2 mCar2firstPosition = new Vector2(500, 250);
        private Vector2 mCarPosition = new Vector2(280, 440);
        private int mFirstLocation = 280;
        private int mMoveCarX = 160;
        private int mVelocityY;
        private double mNextHazardAppearsIn;
        private double mNextLifeBoxAppearsIn;
        private int mCarsRemaining;
        private int mHazardsPassed;
        private int mIncreaseVelocity;
        private double mExitCountDown = 10;  // 游戏开始倒计时
        private int mCartype;   //赛车类型

        private int[] mRoadY = new int[2];
        private List<Hazard> mHazards = new List<Hazard>(); //创建障碍物列表
        private List<Hazard> mLifeBoxs = new List<Hazard>(); //创建医药包列表 

        // 定义随机数 - 比方用来表示障碍物的位置
        private Random mRandom = new Random();

        private SpriteFont mFont;

        //----------------------- Feng ---------------------
        // 自定义枚举类型，表明不同的游戏状态
        private enum State
        {
            TitleScreen, // 初始片头
            Running,
            Crash,           // 碰撞
            AddLife,        // 增加一次生命
            GameOver,
            Success
        }
        //--------------------- Tian --------------------------


        private State mCurrentState = State.TitleScreen;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // 定义游戏窗口大小
            graphics.PreferredBackBufferHeight = 600;
            graphics.PreferredBackBufferWidth = 800;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>

        private SoundEffectInstance sound;
        private Song song;
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            mCar = Content.Load<Texture2D>("Images/Car");
            mCar1 = Content.Load<Texture2D>("Images/Car1");
            mCar2 = Content.Load<Texture2D>("Images/Car2");
            mBackground = Content.Load<Texture2D>("Images/Background");
            mRoad = Content.Load<Texture2D>("Images/Road");
            mHazard = Content.Load<Texture2D>("Images/Hazard");
            mLifeBox = Content.Load<Texture2D>("Images/LifeBox");

            // 定义字体
            mFont = Content.Load<SpriteFont>("MyFont");

            //添加音乐
            this.sound = this.Content.Load<SoundEffect>("Sound/5138").CreateInstance();
            this.song = this.Content.Load<Song>("Sound/F1"); 
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void StartGame()
        {
            mRoadY[0] = 0;
            mRoadY[1] = -1 * mRoad.Height;

            mHazardsPassed = 0;
            mCarsRemaining = 5;//生命值
            if (mCartype == 2)
                mCarsRemaining = 3;
            else if (mCartype == 3)
                mCarsRemaining = 4; //不同类型车具有不同的生命值
            mVelocityY = 3;
            mNextHazardAppearsIn = 1.5; //第一个障碍出现的时间
            mNextLifeBoxAppearsIn = 2; //第一个医药包出现的时间
            mIncreaseVelocity = 5;  // 速度递增

            mHazards.Clear();   //障碍物清零
            mLifeBoxs.Clear();  //医药包清零

            mCurrentState = State.Running;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState aCurrentKeyboardState = Keyboard.GetState();

            //Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                aCurrentKeyboardState.IsKeyDown(Keys.Escape) == true)
            {
                this.Exit();
            }

            switch (mCurrentState)
            {
                case State.TitleScreen:
                    {
                        ExitCountdown(gameTime);
                        //选择车型
                        if (aCurrentKeyboardState.IsKeyDown(Keys.D1) == true && mPreviousKeyboardState.IsKeyDown(Keys.D1) == false)
                        {
                            mCartype = 1;
                            StartGame();
                        }
                        else if (aCurrentKeyboardState.IsKeyDown(Keys.D2) == true && mPreviousKeyboardState.IsKeyDown(Keys.D2) == false)
                        {
                            mCartype = 2;
                            StartGame();
                        }
                        else if (aCurrentKeyboardState.IsKeyDown(Keys.D3) == true && mPreviousKeyboardState.IsKeyDown(Keys.D3) == false)
                        {
                            mCartype = 3;
                            StartGame();
                        }
                        break;
                    }

                case State.Success:
                case State.GameOver:
                    {
                        ExitCountdown(gameTime);
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && mPreviousKeyboardState.IsKeyDown(Keys.Space) == false)
                        {
                            mCurrentState = State.TitleScreen;
                        }
                        break;
                    }

                case State.AddLife:
                    {
                        mLifeBoxs.Clear();
                        mCurrentState = State.Running;
                        break;
                    }
                
                case State.Running:
                    {
                        if (MediaPlayer.GameHasControl && MediaPlayer.State != MediaState.Playing)
                            MediaPlayer.Play(this.song);

                        //If the user has pressed the Spacebar, then make the car switch lanes
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Left) == true && mPreviousKeyboardState.IsKeyDown(Keys.Left) == false)
                        {//Lift
                            mCarPosition.X -= mMoveCarX;
                            if (mCarPosition.X < (mFirstLocation - mMoveCarX))
                                mCarPosition.X += mMoveCarX;
                        }
                        else if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true && mPreviousKeyboardState.IsKeyDown(Keys.Right) == false)
                        {//Right
                            mCarPosition.X += mMoveCarX;
                            if (mCarPosition.X > mFirstLocation + mMoveCarX*2)
                                mCarPosition.X -= mMoveCarX;
                        }
                        else if (aCurrentKeyboardState.IsKeyDown(Keys.Up) == true && mPreviousKeyboardState.IsKeyDown(Keys.Up) == false)
                        {//Up
                            mCarPosition.Y -= 100;
                            if (mCarPosition.Y < 0)
                                mCarPosition.Y += 100;
                        }
                        else if (aCurrentKeyboardState.IsKeyDown(Keys.Down) == true && mPreviousKeyboardState.IsKeyDown(Keys.Down) == false)
                        {//Down
                            mCarPosition.Y += 100;
                            if (mCarPosition.Y > 480)   //初始高度
                                mCarPosition.Y -= 100;
                        }

                        ScrollRoad();
                        //check Harzards
                        foreach (Hazard aHazard in mHazards)
                        {
                            if (CheckCollision(aHazard,1) == true)
                            {
                                break;
                            }

                            MoveHazard(aHazard);
                        }
                        UpdateHazards(gameTime);
                        //check LifeBoxs
                        foreach (Hazard aLifeBox in mLifeBoxs)
                        {
                            if (CheckCollision(aLifeBox,2) == true)
                            {
                                break;
                            }

                            MoveLifeBox(aLifeBox);
                        }
                        UpdateLifeBoxs(gameTime);
                        break;
                    }
                case State.Crash:
                    {
                        //碰撞声音
                        SoundEffect bullet = this.Content.Load<SoundEffect>("sound/5138");
                        SoundEffectInstance bullet1 = bullet.CreateInstance();
                        SoundEffectInstance bullet2 = bullet.CreateInstance();
                        bullet1.Play();
                        bullet2.Play();

                        //If the user has pressed the Space key, then resume driving
                        if (aCurrentKeyboardState.IsKeyDown(Keys.Space) == true && mPreviousKeyboardState.IsKeyDown(Keys.Space) == false)
                        {

                           
                            mHazards.Clear();
                            mLifeBoxs.Clear();
                            mCurrentState = State.Running;
                        }

                        break;
                    }
            }
            mPreviousKeyboardState = aCurrentKeyboardState;

            base.Update(gameTime);
        }

        //----------------------- Feng ---------------------
        // 让路面向后移动（使车辆看起来在往前行）
        private void ScrollRoad()
        {
            //Move the scrolling Road
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                if (mRoadY[aIndex] >= this.Window.ClientBounds.Height) // 检测路面有没有移出游戏窗口
                {
                    int aLastRoadIndex = aIndex;
                    for (int aCounter = 0; aCounter < mRoadY.Length; aCounter++)
                    {
                        if (mRoadY[aCounter] < mRoadY[aLastRoadIndex])
                        {
                            aLastRoadIndex = aCounter;
                        }
                    }
                    mRoadY[aIndex] = mRoadY[aLastRoadIndex] - mRoad.Height; 
                }
            }
            int temp = 0;
            if (mCartype == 1)
                temp = 10;
            else if (mCartype == 2)
                temp = 0;
            else if (mCartype == 3)
                temp = 5;
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                mRoadY[aIndex] += (mVelocityY + temp);;// 改变Y坐标，让路移动；不同类型速度不一样
            }
        }
        //----------------------- Tian ---------------------

        private void MoveHazard(Hazard theHazard)
        {
            int temp = 0;
            if (mCartype == 1)
                temp = 10;
            else if (mCartype == 2)
                temp = 0;
            else if (mCartype == 3)
                temp = 5;
            theHazard.Position.Y += (mVelocityY + temp);    //改变Hazard的Y坐标，让障碍移动；不同类型速度不一样
            if (theHazard.Position.Y > graphics.GraphicsDevice.Viewport.Height && theHazard.Visible == true)
            {// 检测路障有没有移出游戏窗口
                theHazard.Visible = false;
                mHazardsPassed += 1;

                if (mHazardsPassed >= 100) // 如果通过100个障碍物，成功！
                {
                    mCurrentState = State.Success;
                    mExitCountDown = 10;    //游戏结束倒计时
                }

                mIncreaseVelocity -= 1;
                if (mIncreaseVelocity < 0)
                {
                    mIncreaseVelocity = 5;
                    mVelocityY += 1;
                }
            }
        }

        private void UpdateHazards(GameTime theGameTime)
        {
            mNextHazardAppearsIn -= theGameTime.ElapsedGameTime.TotalSeconds; // 游戏运行的时间
            if (mNextHazardAppearsIn < 0)
            {
                int aLowerBound = 24 - (mVelocityY * 2);
                int aUpperBound = 30 - (mVelocityY * 2);

                if (mVelocityY > 10)
                {
                    aLowerBound = 6;
                    aUpperBound = 8;
                }

                // 控制障碍物出现的位置（随机）
                mNextHazardAppearsIn = (double)mRandom.Next(aLowerBound, aUpperBound) / 10;
                AddHazard();
            }
        }

        private void AddHazard()
        {
            int aRoadPosition = mRandom.Next(1, 100);
            int aPosition = 110;
            if ((aRoadPosition % 4) == 1)
            {
                aPosition = 275;
            }
            else if ((aRoadPosition % 4) == 2)
            {
                aPosition = 440;
            }
            else if ((aRoadPosition % 4) == 3)
            {
                aPosition = 590;
            }

            bool aAddNewHazard = true;
            foreach (Hazard aHazard in mHazards)
            {
                if (aHazard.Visible == false)
                {
                    aAddNewHazard = false;
                    aHazard.Visible = true;
                    aHazard.Position = new Vector2(aPosition, -mHazard.Height);
                    break;
                }
            }

            if (aAddNewHazard == true)
            {
                //Add a hazard to the left side of the Road
                Hazard aHazard = new Hazard();
                aHazard.Position = new Vector2(aPosition, -mHazard.Height);

                mHazards.Add(aHazard);
            }
        }

        private void MoveLifeBox(Hazard thelifeBox)
        {
            int temp = 0;
            if (mCartype == 1)
                temp = 10;
            else if (mCartype == 2)
                temp = 0;
            else if (mCartype == 3)
                temp = 5;
            thelifeBox.Position.Y +=  (mVelocityY*2 +temp); //改变LifeBox的Y坐标，让医药包移动；不同类型速度不一样
            if (thelifeBox.Position.Y > graphics.GraphicsDevice.Viewport.Height && thelifeBox.Visible == true)
            {// 检测医药包有没有移出游戏窗口
                thelifeBox.Visible = false;

                mIncreaseVelocity -= 1;
                if (mIncreaseVelocity < 0)
                {
                    mIncreaseVelocity = 5;
                    mVelocityY += 1;
                }
            }
        }

        private void UpdateLifeBoxs(GameTime theGameTime)
        {
            mNextLifeBoxAppearsIn -= (theGameTime.ElapsedGameTime.TotalSeconds /10) ; // 减少医药包出现的概率
            if (mNextLifeBoxAppearsIn < 0)
            {
                int aLowerBound = 24 - (mVelocityY);
                int aUpperBound = 30 - (mVelocityY);

                if (mVelocityY > 10)
                {
                    aLowerBound = 5;
                    aUpperBound = 7;
                }

                // 控制障碍物出现的位置（随机）
                mNextLifeBoxAppearsIn = (double)mRandom.Next(aLowerBound, aUpperBound) / 10;
                AddLifeBox();
            }
        }

        private void AddLifeBox()
        {
            int aRoadPosition = mRandom.Next(1, 100);
            int aPosition = 110;
            if ((aRoadPosition % 3) == 0)
            {
                aPosition = 275;
            }
            else if ((aRoadPosition % 5) == 0)
            {
                aPosition = 440;
            }
            else if ((aRoadPosition % 7) == 0)
            {
                aPosition = 590;
            }

            bool aAddNewLifeBox = true;
            foreach (Hazard aLifeBox in mLifeBoxs)
            {
                if (aLifeBox.Visible == false)
                {
                    aAddNewLifeBox = false;
                    aLifeBox.Visible = true;
                    aLifeBox.Position = new Vector2(aPosition, -mLifeBox.Height);
                    break;
                }
            }

            if (aAddNewLifeBox == true)
            {
                //Add a hazard to the left side of the Road
                Hazard aLifeBox = new Hazard();
                aLifeBox.Position = new Vector2(aPosition, -mLifeBox.Height);

                mLifeBoxs.Add(aLifeBox);
            }
        }
        //----------------------- Feng ------------------------------------------------
        // 检测车辆是否碰到了障碍物
        private bool CheckCollision(Hazard theHazard,int n)
        {
            // 分别计算并使用封闭（包裹）盒给障碍物和车
            BoundingBox aHazardBox = new BoundingBox(new Vector3(theHazard.Position.X, theHazard.Position.Y, 0), new Vector3(theHazard.Position.X + (mHazard.Width * .4f), theHazard.Position.Y + ((mHazard.Height - 50) * .4f), 0));
            BoundingBox aCarBox = new BoundingBox(new Vector3(mCarPosition.X, mCarPosition.Y, 0), new Vector3(mCarPosition.X + (mCar.Width * .2f), mCarPosition.Y + (mCar.Height * .2f), 0));

            if (aHazardBox.Intersects(aCarBox) == true) // 碰上了吗?
            {
                int temp = 5;
                if (mCartype == 2)
                    temp = 3;
                else if (mCartype == 3)
                    temp = 4;

                if (n == 1)
                {//碰到障碍物
                    mCurrentState = State.Crash;
                    mCarsRemaining -= 1;
                }
                else
                {//碰到医药包
                    mCarsRemaining += 1;
                    mCurrentState = State.AddLife;
                }
                if (mCarsRemaining <= 0)
                {
                    mCurrentState = State.GameOver;
                    mExitCountDown = 10;
                }
                else if(mCarsRemaining > temp)
                    mCarsRemaining = temp;
                return true;
            }

            return false;
        }
        //----------------------- Tian ------------------------------------------------------

        private void ExitCountdown(GameTime theGameTime)
        {
            mExitCountDown -= theGameTime.ElapsedGameTime.TotalSeconds;
            if (mExitCountDown < 0)
            {
                this.Exit();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(mBackground, new Rectangle(graphics.GraphicsDevice.Viewport.X, graphics.GraphicsDevice.Viewport.Y, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height), Color.White);

            switch (mCurrentState)
            {
                case State.TitleScreen:
                    {
                        //Draw the display text for the Title screen
                        DrawTextCentered("Drive Fast And Avoid the Oncoming Obstacles", 100);
                        spriteBatch.Draw(mCar, mCarfirstPosition, new Rectangle(0, 0, mCar.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.3f, SpriteEffects.None, 0);
                        spriteBatch.Draw(mCar1, mCar1firstPosition, new Rectangle(0, 0, mCar1.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.3f, SpriteEffects.None, 0);
                        spriteBatch.Draw(mCar2, mCar2firstPosition, new Rectangle(0, 0, mCar2.Width, mCar.Height), Color.White, 0, new Vector2(0, 0), 0.3f, SpriteEffects.None, 0);
                        DrawTextCentered("Choose One Type You Like: Press'1' or Press'2' or Press'3'", 400);
                        DrawTextCentered("Exit in " + ((int)mExitCountDown).ToString(), 475);

                        break;
                    }

                default:
                    {
                        DrawRoad();
                        DrawHazards();
                        DrawLifeBoxs();

                        if (mCartype == 1)
                        {
                            mCartemp = mCar;
                        }
                        else if (mCartype == 2)
                        {
                            mCartemp = mCar1;
                        }
                        else if (mCartype == 3)
                        {
                            mCartemp = mCar2;
                        }
                        spriteBatch.Draw(mCartemp, mCarPosition, new Rectangle(0, 0, mCartemp.Width, mCartemp.Height), Color.White, 0, new Vector2(0, 0), 0.2f, SpriteEffects.None, 0);
                        spriteBatch.DrawString(mFont, "LifeRemaining", new Vector2(5, 520), Color.Red, 0, new Vector2(0, 0), 0.6f, SpriteEffects.None, 0);
                        for (int aCounter = 0; aCounter < mCarsRemaining; aCounter++)
                        {
                            spriteBatch.Draw(mCartemp, new Vector2(15 + (30 * aCounter), 550), new Rectangle(0, 0, mCartemp.Width, mCartemp.Height), Color.White, 0, new Vector2(0, 0), 0.05f, SpriteEffects.None, 0);
                        }
                        spriteBatch.DrawString(mFont, "Grade: " + mHazardsPassed.ToString(), new Vector2(5, 25), Color.Blue, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);

                        if (mCurrentState == State.Crash)
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Crash!", 200);
                            DrawTextCentered("Press 'Space' to continue driving.", 260);
                        }
                        else if (mCurrentState == State.GameOver)
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Game Over.", 200);
                            DrawTextCentered("Press 'Space' to try again.", 260);
                            DrawTextCentered("Exit in " + ((int)mExitCountDown).ToString(), 400);

                        }
                        else if (mCurrentState == State.Success)
                        {
                            DrawTextDisplayArea();

                            DrawTextCentered("Congratulations!", 200);
                            DrawTextCentered("Press 'Space' to play again.", 260);
                            DrawTextCentered("Exit in " + ((int)mExitCountDown).ToString(), 400);
                        }

                        break;
                    }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawRoad()
        {
            for (int aIndex = 0; aIndex < mRoadY.Length; aIndex++)
            {
                if (mRoadY[aIndex] > mRoad.Height * -1 && mRoadY[aIndex] <= this.Window.ClientBounds.Height)
                {
                    spriteBatch.Draw(mRoad, new Rectangle((int)((this.Window.ClientBounds.Width - mRoad.Width) / 2 - 18), mRoadY[aIndex], mRoad.Width, mRoad.Height + 5), Color.White);
                }
            }
        }
        
        private void DrawHazards()
        {
            foreach (Hazard aHazard in mHazards)
            {
                if (aHazard.Visible == true)
                {
                    spriteBatch.Draw(mHazard, aHazard.Position, new Rectangle(0, 0, mHazard.Width, mHazard.Height), Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 0);
                }
            }
        }

        private void DrawLifeBoxs()
        {
            foreach (Hazard aLifeBox in mLifeBoxs)
            {
                if (aLifeBox.Visible == true)
                {
                    spriteBatch.Draw(mLifeBox, aLifeBox.Position, new Rectangle(0, 0, mLifeBox.Width, mLifeBox.Height), Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 0);
                }
            }
        }

        private void DrawTextDisplayArea()
        {
            int aPositionX = (int)((graphics.GraphicsDevice.Viewport.Width / 2) - (450 / 2));
            spriteBatch.Draw(mBackground, new Rectangle(aPositionX, 75, 450, 400), Color.White);
        }

        private void DrawTextCentered(string theDisplayText, int thePositionY)
        {
            Vector2 aSize = mFont.MeasureString(theDisplayText);
            int aPositionX = (int)((graphics.GraphicsDevice.Viewport.Width / 2) - (aSize.X / 2));

            spriteBatch.DrawString(mFont, theDisplayText, new Vector2(aPositionX, thePositionY), Color.Beige, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
            spriteBatch.DrawString(mFont, theDisplayText, new Vector2(aPositionX + 1, thePositionY + 1), Color.Brown, 0, new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);
        }
    }
}
