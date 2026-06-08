import { Scene, GameObjects } from 'phaser';

export class MainMenu extends Scene
{
    background: GameObjects.Image;
    logo: GameObjects.Image;
    title: GameObjects.Text;
    scoreText: GameObjects.Text;
    highScoreText: GameObjects.Text;
    instructionsText: GameObjects.Text;

    private highScore: number = 0;
    constructor ()
    {
        super('MainMenu');
    }

    create ()
    {
        this.background = this.add.image(512, 384, 'background');


        this.logo = this.add.image(512, 200, 'logo');


        this.title = this.add.text(512, 320, '2048', {
            fontFamily: 'Arial Black', fontSize: 80, color: '#ffffff',
            stroke: '#000000', strokeThickness: 8,
            align: 'center'
        }).setOrigin(0.5);

        // Load high score from localStorage
        const savedHighScore = localStorage.getItem('2048_best_score');
        this.highScore = savedHighScore ? parseInt(savedHighScore) : 0;

        this.highScoreText = this.add.text(512, 400, `High Score: ${this.highScore}`, {
            fontFamily: 'Arial', fontSize: 28, color: '#ffd700',
            stroke: '#000000', strokeThickness: 4,
            align: 'center'
        }).setOrigin(0.5);

        this.scoreText = this.add.text(512, 440, 'Score: 0', {
            fontFamily: 'Arial', fontSize: 24, color: '#aaaaaa',
            align: 'center'
        }).setOrigin(0.5);
        this.instructionsText = this.add.text(512, 520, 'Use Arrow Keys or Swipe to Play\nClick to Start', {
            fontFamily: 'Arial', fontSize: 20, color: '#cccccc',
            align: 'center'
        }).setOrigin(0.5);

        this.input.once('pointerdown', () => {
            this.scene.start('Game');
        });
    }
    updateHighScore(score: number)
    {
        if (score > this.highScore)
        {
            this.highScore = score;
            localStorage.setItem('2048_best_score', score.toString());
        }
    }
}

export { MainMenu as default };
