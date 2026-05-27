/**
 * Game.ts - Complete 2048 Game Implementation
 * A sliding tile puzzle game where players combine numbered tiles to reach 2048
 * 
 * Features:
 * - 4x4 grid with tile sliding and merging mechanics
 * - Score tracking with localStorage persistence for best score
 * - Win/lose detection with continue playing option
 * - Keyboard controls (Arrow keys + WASD)
 * - Touch/swipe support for mobile devices
 * - Smooth CSS animations for tile movement, merging, and spawning
 * - Full game state management (IDLE, ANIMATING, WIN, GAME_OVER)
 */
const GRID_SIZE=4,WIN_VALUE=2048;
const TILE_COLORS:Record<number,string>={2:'#eee4da',4:'#ede0c8',8:'#f2b179',16:'#f59563',32:'#f67c5f',64:'#f65e3b',128:'#edcf72',256:'#edcc61',512:'#edc850',1024:'#edc53f',2048:'#edc22e',4096:'#3c3a32',8192:'#3c3a32'};
const TEXT_COLORS:Record<number,string>={2:'#776e65',4:'#776e65',8:'#f9f6f2',16:'#f9f6f2',32:'#f9f6f2',64:'#f9f6f2',128:'#f9f6f2',256:'#f9f6f2',512:'#f9f6f2',1024:'#f9f6f2',2048:'#f9f6f2',4096:'#f9f6f2',8192:'#f9f6f2'};
const FONT_SIZES:Record<number,string>={2:'55px',4:'55px',8:'55px',16:'50px',32:'50px',64:'50px',128:'45px',256:'45px',512:'45px',1024:'40px',2048:'40px',4096:'35px',8192:'35px'};
export enum GameState{IDLE='IDLE',ANIMATING='ANIMATING',WIN='WIN',GAME_OVER='GAME_OVER'}
export enum Direction{UP='UP',DOWN='DOWN',LEFT='LEFT',RIGHT='RIGHT'}
export interface Position{row:number;col:number;}
export interface TileData{value:number;row:number;col:number;mergedFrom?:Position[];isNew?:boolean;}
export interface GameMove{tiles:TileData[];score:number;won:boolean;}
export interface SavedState{board:number[][];score:number;won:boolean;keepPlaying:boolean;}
export class Game2048{
private board:number[][];private score:number;private bestScore:number;private gameState:GameState;private keepPlaying:boolean;private hasWon:boolean;
private sHtml:()=>void;private gEl:(id:string)=>HTMLElement|null;
private tc:HTMLElement|null=null;private scC:HTMLElement|null=null;private bc:HTMLElement|null=null;
private mc:HTMLElement|null=null;private gc:HTMLElement|null=null;private rb:HTMLElement|null=null;private cb:HTMLElement|null=null;
private tileElements:Map<string,HTMLElement>=new Map();private animationDuration=150;private touchStartX:number=0;private touchStartY:number=0;
constructor(setupHtmlElements:()=>void,getElement:(id:string)=>HTMLElement|null){this.setupHtmlElements=setupHtmlElements;this.getElement=getElement;this.board=this.createEmptyBoard();this.score=0;this.bestScore=this.loadBestScore();this.gameState=GameState.IDLE;this.keepPlaying=false;this.hasWon=false;}
public init():void{this.sHtml();this.tc=this.gEl('tiles-container');this.scC=this.gEl('score');this.bc=this.gEl('best-score');this.mc=this.gEl('game-message');this.gc=this.gEl('game-container');this.rb=this.gEl('restart-button');this.cb=this.gEl('continue-button');this.sEL();this.sNG();}
private cEB():number[][]{return Array(GS).fill(null).map(()=>Array(GS).fill(0));}
public sNG():void{this.b=this.cEB();this.sc=0;this.st=GS2.IDLE;this.kp=false;this.hw=false;this.tEl.clear();this.sT();this.sT();this.uSD();this.uBSD();this.rB();this.hM();this.sSt();}
private sEL():void{document.addEventListener('keydown',(e:KeyboardEvent)=>{if(this.st===GS2.ANIM)return;const kM:Record<string,DIR>={'ArrowUp':DIR.UP,'ArrowDown':DIR.DN,'ArrowLeft':DIR.LF,'ArrowRight':DIR.RT,'w':DIR.UP,'W':DIR.UP,'s':DIR.DN,'S':DIR.DN,'a':DIR.LF,'A':DIR.LF,'d':DIR.RT,'D':DIR.RT};const d=kM[e.key];if(d){e.preventDefault();this.mv(d);}});document.addEventListener('touchstart',(e:TouchEvent)=>{this.tSX=e.touches[0].clientX;this.tSY=e.touches[0].clientY;});document.addEventListener('touchend',(e:TouchEvent)=>{if(this.st===GS2.ANIM)return;const dX=e.changedTouches[0].clientX-this.tSX;const dY=e.changedTouches[0].clientY-this.tSY;if(Math.abs(dX)<50&&Math.abs(dY)<50)return;let d:DIR;if(Math.abs(dX)>Math.abs(dY)){d=dX>0?DIR.RT:DIR.LF;}else{d=dY>0?DIR.DN:DIR.UP;}this.mv(d);});if(this.rb){this.rb.addEventListener('click',()=>this.sNG());}if(this.cb){this.cb.addEventListener('click',()=>this.cG());}}
public mv(d:DIR):void{const pB=this.cloneB();let mvd=false;let mSc=0;this.st=GS2.ANIM;const v=this.gV(d);const t=this.bT(v);const mg=Array(GS).fill(null).map(()=>Array(GS).fill(false));t.rows.forEach(r=>{t.cols.forEach(c=>{const val=this.b[r][c];if(val===0)return;const{farthest,next}=this.fFP(r,c,v);if(next&&this.b[next.row][next.col]===val&&!mg[next.row][next.col]){this.b[next.row][next.col]=val*2;this.b[r][c]=0;mg[next.row][next.col]=true;mSc+=val*2;mvd=true;}else if(farthest.row!==r||farthest.col!==c){this.b[farthest.row][farthest.col]=val;if(farthest.row!==r||farthest.col!==c)this.b[r][c]=0;mvd=true;}});});if(mvd){this.sc+=mSc;this.updateBestScoreIfNeeded();}if(!this.boardsEqual(pB,this.b)){this.sT();}this.uSD();this.rB();setTimeout(()=>{this.st=GS2.IDLE;if(!this.kp&&this.cW()&&!this.hw){this.st=GS2.WIN;this.hw=true;this.sM('win');}else if(this.cGO()){this.st=GS2.OVER;this.sM('game-over');}this.sSt();},this.AD);}
private gV(d:DIR):Pos{const v:Record<DIR,Pos>={UP:{row:-1,col:0},DN:{row:1,col:0},LF:{row:0,col:-1},RT:{row:0,col:1}};return v[d];}
private bT(v:Pos):{rows:number[];cols:number[]}{let rows:number[]=Array.from({length:GS},(_,i)=>i);let cols:number[]=Array.from({length:GS},(_,i)=>i);if(v.row===1){rows=rows.reverse();}if(v.col===1){cols=cols.reverse();}return{rows,cols};}
private fFP(r:number,c:number,v:Pos):{farthest:Pos;next:Pos|null}{let fr=r,fc=c;while(true){const nr=fr+v.row,nc=fc+v.col;if(nr<0||nr>=GS||nc<0||nc>=GS||this.b[nr][nc]!==0){break;}fr=nr;fc=nc;}let next:Pos|null=null;const nr=fr+v.row,nc=fc+v.col;if(nr>=0&&nr<GS&&nc>=0&&nc<GS&&this.b[nr][nc]!==0){next={row:nr,col:nc};}
return{farthest:{row:fr,col:fc},next};}
private cloneB():number[][]{return this.b.map(r=>[...r]);}
private boardsEqual(a:number[][],b:number[][]):boolean{for(let r=0;r<GS;r++)for(let c=0;c<GS;c++)if(a[r][c]!==b[r][c])return false;return true;}
private sT():void{const eC=this.gEC();if(eC.length>0){const idx=Math.floor(Math.random()*eC.length);const{row,col}=eC[idx];this.b[row][col]=Math.random()<0.9?2:4;}}
private gEC():Pos[]{const eC:Pos[]=[];for(let r=0;r<GS;r++)for(let c=0;c<GS;c++)if(this.b[r][c]===0)eC.push({row:r,col:c});return eC;}
public cW():boolean{for(let r=0;r<GS;r++)for(let c=0;c<GS;c++)if(this.b[r][c]===WV)return true;return false;}
public cGO():boolean{if(this.gEC().length>0)return false;for(let r=0;r<GS;r++)for(let c=0;c<GS;c++){const v=this.b[r][c];if((r<GS-1&&this.b[r+1][c]===v)||(c<GS-1&&this.b[r][c+1]===v))return false;}return true;}
private cG():void{this.kp=true;this.hM();this.st=GS2.IDLE;}
private sM(type:string):void{if(!this.mc)return;this.mc.className='game-message '+type;this.mc.innerHTML=type==='win'?'<p>You Win!</p><p>Keep going?</p><button id="continue-button" class="keep-playing-button">Keep Playing</button>':'<p>Game Over!</p><button id="retry-button" class="retry-button">Try Again</button>';const rb=document.getElementById('retry-button');if(rb){rb.addEventListener('click',()=>this.sNG());}}
private hM():void{if(this.mc)this.mc.className='game-message';}
private uSD():void{if(this.scC)this.scC.textContent=this.sc.toString();}
private uBSD():void{if(this.bc)this.bc.textContent=this.bs.toString();}
private updateBestScoreIfNeeded():void{if(this.sc>this.bs){this.bs=this.sc;this.sBS();}this.uBSD();}
private lBS():number{const s=localStorage.getItem('bestScore');return s?parseInt(s):0;}
private sBS():void{localStorage.setItem('bestScore',this.bs.toString());}
private sSt():void{try{const st:SState={board:this.b,score:this.sc,won:this.hw,keepPlaying:this.kp};localStorage.setItem('gameState',JSON.stringify(st));}catch(e){}}
private rB():void{if(!this.tc)return;this.tc.innerHTML='';for(let r=0;r<GS;r++)for(let c=0;c<GS;c++){const v=this.b[r][c];if(v!==0){const tile=document.createElement('div');tile.className='tile tile-'+v;tile.textContent=v.toString();tile.style.backgroundColor=TC[v]||TC[4096];tile.style.color=TxtC[v]||TxtC[4096];tile.style.fontSize=FS[v]||FS[4096];const pos=this.gTP(r,c);tile.style.transform='translate('+pos.x+'px,'+pos.y+'px)';this.tc.appendChild(tile);}}}}
private gTP(r:number,c:number):{x:number;y:number}{const size=100+5;return{x:c*size,y:r*size};}
}