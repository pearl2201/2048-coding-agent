import { Scene, GameObjects } from 'phaser';

export class GameOver extends Scene
{
    camera: Phaser.Cameras.Scene2D.Camera;
    background: GameObjects.Image;
    gameover_text: GameObjects.Text;
    score_text: GameObjects.Text;
    restart_text: GameObjects.Text;
    finalScore: number = 0;

    constructor ()
    {
        super('GameOver');
    }

    init(data: { score: number })
    {
        this.finalScore = data.score || 0;
        
        // Save high score if it's a new record
        const savedHighScore = localStorage.getItem('2048_highscore');
        const currentHigh = savedHighScore ? parseInt(savedHighScore) : 0;
        if (this.finalScore > currentHigh) {
            localStorage.setItem('2048_highscore', this.finalScore.toString());
        }
    }

    create ()
    {
        this.camera = this.cameras.main;
        this.camera.setBackgroundColor(0x1a2e);
        // Semi-transparent background overlay
        this.add.rectangle(512, 384, 1024, 768, 0x000000, 0.7);
        this.gameover_text = this.add.text(512, 200, 'GAME OVER', {
            fontFamily: 'Arial Black', fontSize: 64, color: '#ffffff',
            stroke: '#000000', strokeThickness: 8,
            align: 'center'
        }).setOrigin(0.5);

        this.score_text = this.add.text(512, 320, `Final Score: ${this.finalScore}`, {
            fontFamily: 'Arial Black', fontSize: 48, color: '#ffd700',
            stroke: '#000000', strokeThickness: 6,
            align: 'center'
        }).setOrigin(0.5);

        this.restart_text = this.add.text(512, 450, 'Click to Restart', {
            fontFamily: 'Arial', fontSize: 32, color: '#ffffff',
            align: 'center'
        }).setOrigin(0.5).setInteractive();

        this.restart_text.on('pointerdown', () => {
            this.scene.start('MainMenu');
        });

        this.restart_text.on('pointerover', () => {
            this.restart_text.setColor('#ffff00');
        });


        this.restart_text.on('pointerout', () => {
            this.restart_text.setColor('#ffffff');
        });
    }
}