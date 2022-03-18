import React from "react";
import { Link } from "react-router-dom";

import styles from "./HowToPlay.Module.css";

export default class HowToPlay extends React.Component {
	render() {
		return (
			<div className={styles.container}>
				<h1>
					How to Play Draughts
				</h1>

				<h2>Structure of the Game</h2>
				<p>
					At the start of the game both players have 3 rows of pieces.<br />
					The objective of the game is to eliminate the opponents pieces, but the game is also won if your opponent doesn't have any vailid moves.
				</p>

				<h2>Moving the Pieces</h2>
				<p>
					Normal pieces can move one square diagonally forwards.<br />
					On your turn, select the piece that you would like to move.<br />
					The tiles it's able to move to will be highlighted with a <b><span style={{ color: "#89b396" }}>green</span></b> marker. Select one of these tiles to submit your move.
				</p>

				<h2>Eliminating Opponent Pieces</h2>
				<p>
					If one of your pieces is highlighted in <b><span style={{ color: "#a689b3" }}>purple</span></b>, it means that it has a jumping move where it can jump over an opponent piece to eliminate it.<br />
					If a jumping move is possible, you <b>must</b> take it.<br />
					After you jump an opponent piece you can jump another opponent piece to form a jump chain, but all jumps must be performed by the same piece.
				</p>

				<h2>Promoting to Kings</h2>
				<p>
					If a piece travels all the way to the other side of the board, it is promoted to a king.<br />
					A king can move backwards as well as forwards, and this also applies to jumping moves.<br />
				</p>

				<Link className={styles.link} to="/play">Find a game ➔</Link>
			</div>
		);
	}
}
