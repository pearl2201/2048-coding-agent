import { Scene, GameObjects } from 'phaser';

const GRID_SIZE = 4;
const CELL_SIZE = 100;
const CELL_GAP = 8;
const GRID_PADDING = 16;
const WIN_VALUE = 2048;

const TILE_COLORS: Record<number, number> = {
    2: 0xeee4da,
    4: 0xede0c8,
    8: 0xf2b179,
    16: 0xf59563,
    32: 0xf67c5f,
    64: 0xf65e3b,
    128: 0xedcf72,
    256: 0xedcc61,
    512: 0xedc850,
    1024: 0xedc53f,
    2048: 0xedc22e,
    4096: 0x3c3a32,
    8192: 0x3c3a32,
    16384: 0x280005,
    32768: 0x28000d,
    65536: 0x28001a,
};

const TEXT_COLORS: Record<number, string> = {
    2: '#776e65',
    4: '#776e65',
    8: '#f9f6f2',
    16: '#f9f6f2',
    32: '#f9f6f2',
    64: '#f9f6f2',
    128: '#f9f6f2',
    256: '#f9f6f2',
    512: '#f9f6f2',
    1024: '#f9f6f2',
    2048: '#f9f6f2',
    4096: '#f9f6f2',
    8192: '#f9f6f2',
    16384: '#f9f6f2',
    32768: '#f9f6f2',
    65536: '#f9f6f2',
};

const FONT_SIZES: Record<number, string> = {
    2: '50px',
    4: '50px',
    8: '50px',
    16: '45px',
    32: '45px',
    64: '45px',
    128: '40px',
    256: '40px',
    512: '40px',
    1024: '35px',
    2048: '35px',
    4096: '30px',
    8192: '30px',
    16384: '25px',
    32768: '25px',
    65536: '25px',
};

enum Direction {
    UP = 'UP',
    DOWN = 'DOWN',
    LEFT = 'LEFT',
    RIGHT = 'RIGHT',
}

interface Position {
    row: number;
    col: number;
}

interface TileSprite {
    rect: GameObjects.Rectangle;
    text: GameObjects.Text;
    row: number;
    col: number;
    value: number;
}

export class Game extends Scene {
    private gridContainer!: GameObjects.Container;
    private tiles: TileSprite[] = [];
    private board: number[][] = [];
    private score: number = 0;
    private bestScore: number = 0;
    private scoreText!: GameObjects.Text;
    private bestScoreText!: GameObjects.Text;
    private isAnimating: boolean = false;
    private hasWon: boolean = false;
    private keepPlaying: boolean = false;
    private touchStartX: number = 0;
    private touchStartY: number = 0;
    private gridOffsetY: number = 100;

    constructor() {
        super('Game');
    }

    create(): void {
        const savedBestScore = localStorage.getItem('2048_bestscore');
        this.bestScore = savedBestScore ? parseInt(savedBestScore) : 0;

        this.createBackground();
        this.createScoreDisplay();
        this.createGrid();
        this.setupInput();
        this.startNewGame();
    }

    private createBackground(): void {
        const bg = this.add.rectangle(512, 384, 1024, 768, 0x028af8);
        bg.setInteractive();
    }

    private createScoreDisplay(): void {
        const rect = this.add.rectangle(512, 50, 400, 80, 0x1a2e, 0.8);
        rect.setRounded(15);
        this.add.text(300, 30, 'SCORE', {
            fontFamily: 'Arial',
            fontSize: '16px',
            color: '#ffffff',
        }).setOrigin(0.5);
        this.scoreText = this.add.text(300, 65, '0', {
            fontFamily: 'Arial Black',
            fontSize: '32px',
            color: '#ffffff',
        }).setOrigin(0.5);
        this.add.text(700, 30, 'BEST', {
            fontFamily: 'Arial',
            fontSize: '16px',
            color: '#ffffff',
        }).setOrigin(0.5);
        this.bestScoreText = this.add.text(700, 65, this.bestScore.toString(), {
            fontFamily: 'Arial Black',
            fontSize: '32px',
            color: '#ffd700',
        }).setOrigin(0.5);
        this.add.text(512, 740, 'Use Arrow Keys or WASD to move tiles', {
            fontFamily: 'Arial',
            fontSize: '18px',
            color: '#cccccc',
        }).setOrigin(0.5);
    }

    private createGrid(): void {
        this.gridContainer = this.add.container(0, 0);
        const gridWidth = GRID_SIZE * (CELL_SIZE + CELL_GAP) - CELL_GAP + 2 * GRID_PADDING;
        const gridHeight = GRID_SIZE * (CELL_SIZE + CELL_GAP) - CELL_GAP + 2 * GRID_PADDING;
        const offsetX = (1024 - gridWidth) / 2;
        const offsetY = this.gridOffsetY;
        const gridBg = this.add.rectangle(
            offsetX + gridWidth / 2,
            offsetY + gridHeight / 2,
            gridWidth,
            gridHeight,
            0xbbada0
        );
        gridBg.setRounded(8);
        this.gridContainer.add(gridBg);
        for (let row = 0; row < GRID_SIZE; row++) {
            for (let col = 0; col < GRID_SIZE; col++) {
                const x = offsetX + GRID_PADDING + col * (CELL_SIZE + CELL_GAP) + CELL_SIZE / 2;
                const y = offsetY + GRID_PADDING + row * (CELL_SIZE + CELL_GAP) + CELL_SIZE / 2;
                const cell = this.add.rectangle(x, y, CELL_SIZE, CELL_SIZE, 0xcdc1b4);
                cell.setRounded(4);
                this.gridContainer.add(cell);
            }
        }
    }

    private getTilePosition(row: number, col: number): { x: number; y: number } {
        const gridWidth = GRID_SIZE * (CELL_SIZE + CELL_GAP) - CELL_GAP + 2 * GRID_PADDING;
        const offsetX = (1024 - gridWidth) / 2;
        const offsetY = this.gridOffsetY;
        return {
            x: offsetX + GRID_PADDING + col * (CELL_SIZE + CELL_GAP) + CELL_SIZE / 2,
            y: offsetY + GRID_PADDING + row * (CELL_SIZE + CELL_GAP) + CELL_SIZE / 2,
        };
    }

    private setupInput(): void {
        this.input.keyboard?.on('keydown', (event: KeyboardEvent) => {
            if (this.isAnimating) return;
            let direction: Direction | null = null;
            if (event.key === 'ArrowUp' || event.key === 'w' || event.key === 'W') direction = Direction.UP;
            else if (event.key === 'ArrowDown' || event.key === 's' || event.key === 'S') direction = Direction.DOWN;
            else if (event.key === 'ArrowLeft' || event.key === 'a' || event.key === 'A') direction = Direction.LEFT;
            else if (event.key === 'ArrowRight' || event.key === 'd' || event.key === 'D') direction = Direction.RIGHT;
            if (direction) { event.preventDefault(); this.move(direction); }
        });
        this.input.on('pointerdown', (pointer: Phaser.Input.Pointer) => { this.touchStartX = pointer.x; this.touchStartY = pointer.y; });
        this.input.on('pointerup', (pointer: Phaser.Input.Pointer) => {
            if (this.isAnimating) return;
            const deltaX = pointer.x - this.touchStartX;
            const deltaY = pointer.y - this.touchStartY;
            if (Math.abs(deltaX) < 30 && Math.abs(deltaY) < 30) return;
            let direction: Direction;
            if (Math.abs(deltaX) > Math.abs(deltaY)) { direction = deltaX > 0 ? Direction.RIGHT : Direction.LEFT; }
            else { direction = deltaY > 0 ? Direction.DOWN : Direction.UP; }
            this.move(direction);
        });
    }

    private startNewGame(): void {
        this.board = this.createEmptyBoard();
        this.tiles = [];
        this.score = 0;
        this.hasWon = false;
        this.keepPlaying = false;
        this.isAnimating = false;
        this.updateScore();
        this.spawnTile();
        this.spawnTile();
    }

    private createEmptyBoard(): number[][] { return Array(GRID_SIZE).fill(null).map(() => Array(GRID_SIZE).fill(0)); }


    private spawnTile(): void {
        const emptyCells = this.getEmptyCells();
        if (emptyCells.length === 0) return;
        const randomCell = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        this.board[randomCell.row][randomCell.col] = Math.random() < 0.9 ? 2 : 4;
        this.createTileSprite(randomCell.row, randomCell.col, this.board[randomCell.row][randomCell.col], true);
    }

    private getEmptyCells(): Position[] {
        const emptyCells: Position[] = [];
        for (let row = 0; row < GRID_SIZE; row++) { for (let col = 0; col < GRID_SIZE; col++) { if (this.board[row][col] === 0) { emptyCells.push({ row, col }); } } }
        return emptyCells;
    }

    private createTileSprite(row: number, col: number, value: number, isNew: boolean = false): void {
        const pos = this.getTilePosition(row, col);
        const color = TILE_COLORS[value] || TILE_COLORS[4096];
        const textColor = TEXT_COLORS[value] || TEXT_COLORS[4096];
        const fontSize = FONT_SIZES[value] || FONT_SIZES[4096];
        const rect = this.add.rectangle(pos.x, pos.y, CELL_SIZE, CELL_SIZE, color);
        rect.setRounded(4);
        const text = this.add.text(pos.x, pos.y, value.toString(), { fontFamily: 'Arial Black', fontSize: fontSize, color: textColor }).setOrigin(0.5);
        if (isNew) { rect.setScale(0); text.setScale(0); this.tweens.add({ targets: [rect, text], scale: 1, duration: 150, ease: 'Back.out' }); }
        const tileSprite: TileSprite = { rect, text, row, col, value };
        this.tiles.push(tileSprite);
    }

    private move(direction: Direction): void {
        const previousBoard = this.cloneBoard();
        let moved = false;
        let mergeScore = 0;
        const mergedPositions: boolean[][] = Array(GRID_SIZE).fill(null).map(() => Array(GRID_SIZE).fill(false));
        const vector = this.getVector(direction);
        const traversals = this.buildTraversals(vector);
        traversals.rows.forEach((row) => {
            traversals.cols.forEach((col) => {
                const value = this.board[row][col];
                if (value === 0) return;
                const { farthest, next } = this.findFarthestPosition(row, col, vector);
                if (next && this.board[next.row][next.col] === value && !mergedPositions[next.row][next.col]) {
                    this.board[next.row][next.col] = value * 2;
                    this.board[row][col] = 0;
                    mergedPositions[next.row][next.col] = true;
                    mergeScore += value * 2;
                    moved = true;
                } else if (farthest.row !== row || farthest.col !== col) {
                    this.board[farthest.row][farthest.col] = value;
                    this.board[row][col] = 0;
                    moved = true;
                }
            });
        });
        if (moved) {
            this.score += mergeScore;
            this.updateScore();
            this.updateBestScore();
            this.animateTiles(previousBoard, () => {
                this.clearTiles();
                this.renderTiles();
                this.spawnTile();
                this.checkGameState();
            });
        }
    }

    private getVector(direction: Direction): Position {
        const map: Record<Direction, Position> = { [Direction.UP]: { row: -1, col: 0 }, [Direction.DOWN]: { row: 1, col: 0 }, [Direction.LEFT]: { row: 0, col: -1 }, [Direction.RIGHT]: { row: 0, col: 1 } };
        return map[direction];
    }


    private buildTraversals(vector: Position): { rows: number[]; cols: number[] } {
        let rows: number[] = Array.from({ length: GRID_SIZE }, (_, i) => i);
        let cols: number[] = Array.from({ length: GRID_SIZE }, (_, i) => i);
        if (vector.row === 1) rows = rows.reverse();
        if (vector.col === 1) cols = cols.reverse();
        return { rows, cols };
    }

    private findFarthestPosition(row: number, col: number, vector: Position): { farthest: Position; next: Position | null } {
        let previous: Position;
        let farthest: Position = { row, col };
        do {
            previous = farthest;
            farthest = { row: farthest.row + vector.row, col: farthest.col + vector.col };
        } while (this.withinBounds(farthest) && this.board[farthest.row][farthest.col] === 0);
        let next: Position | null = null;
        if (this.withinBounds(farthest) && this.board[farthest.row][farthest.col] !== 0) { next = farthest; }
        return { farthest: previous, next };
    }

    private withinBounds(pos: Position): boolean { return pos.row >= 0 && pos.row < GRID_SIZE && pos.col >= 0 && pos.col < GRID_SIZE; }
    private cloneBoard(): number[][] { return this.board.map((row) => [...row]); }
    private animateTiles(previousBoard: number[][], onComplete: () => void): void {
        this.isAnimating = true;
        let completedAnimations = 0;
        const totalAnimations = this.tiles.length;
        this.tiles.forEach((tile) => {
            const newPos = this.getTilePosition(tile.row, tile.col);
            if (tile.rect.x !== newPos.x || tile.rect.y !== newPos.y) {
                this.tweens.add({
                    targets: [tile.rect, tile.text],
                    x: newPos.x,
                    y: newPos.y,
                    duration: 100,
                    ease: 'Linear',
                    onComplete: () => { completedAnimations++; if (completedAnimations >= totalAnimations) { onComplete(); } }
                });
            } else { completedAnimations++; }
        });
        if (completedAnimations >= totalAnimations) { onComplete(); }
    }

    private clearTiles(): void {
        this.tiles.forEach((tile) => { tile.rect.destroy(); tile.text.destroy(); });
        this.tiles = [];
    }
    private renderTiles(): void {
        for (let row = 0; row < GRID_SIZE; row++) {
            for (let col = 0; col < GRID_SIZE; col++) {
                if (this.board[row][col] !== 0) { this.createTileSprite(row, col, this.board[row][col], false); }
            }
        }
    }
    private updateScore(): void { if (this.scoreText) { this.scoreText.setText(this.score.toString()); } }
    private updateBestScore(): void {
        if (this.score > this.bestScore) {
            this.bestScore = this.score;
            if (this.bestScoreText) { this.bestScoreText.setText(this.bestScore.toString()); }
            localStorage.setItem('2048_bestscore', this.bestScore.toString());
        }
    }
    private checkGameState(): void {
        this.isAnimating = false;
        if (!this.hasWon && this.checkWin()) {
            this.hasWon = true;
            this.showMessage('You Win!', () => { this.keepPlaying = true; });
        } else if (this.checkGameOver()) {
            this.showGameOver();
        }
    }
    private checkWin(): boolean {
        for (let row = 0; row < GRID_SIZE; row++) { for (let col = 0; col < GRID_SIZE; col++) { if (this.board[row][col] === WIN_VALUE) { return true; } } }
        return false;
    }
    private checkGameOver(): boolean {
        if (this.getEmptyCells().length > 0) { return false; }
        for (let row = 0; row < GRID_SIZE; row++) {
            for (let col = 0; col < GRID_SIZE; col++) {
                const value = this.board[row][col];
                if (row < GRID_SIZE - 1 && this.board[row + 1][col] === value) { return false; }
                if (col < GRID_SIZE - 1 && this.board[row][col + 1] === value) { return false; }
            }
        }
        return true;
    }
    private showMessage(text: string, onContinue: () => void): void {
        const overlay = this.add.rectangle(512, 384, 1024, 768, 0x000000, 0.5);
        const messageContainer = this.add.container(0, 0);
        const bg = this.add.rectangle(512, 384, 400, 200, 0xffffff);
        bg.setRounded(16);
        const titleText = this.add.text(512, 320, text, { fontFamily: 'Arial Black', fontSize: '48px', color: '#776e65' }).setOrigin(0.5);
        const continueText = this.add.text(512, 420, 'Click to Continue', { fontFamily: 'Arial', fontSize: '24px', color: '#776e65' }).setOrigin(0.5).setInteractive();
        messageContainer.add([overlay, bg, titleText, continueText]);
        continueText.on('pointerdown', () => { messageContainer.destroy(); onContinue(); });
    }
    private showGameOver(): void { this.scene.start('GameOver', { score: this.score }); 
}
}
