import React from "react";

import Tile from "./Tile";

import styles from "./Board.Module.css";

export default class Board extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			selectedTile: null
		};
	}

	render() {
		return (
			<div className={styles.container}>
				<div className={styles.board}>
					{this.renderRows()}
				</div>
			</div>
		);
	}

	renderRows() {
		var rows = [];

		var possibleSelectedTileMoves = this.state.selectedTile && this.props.board.validMoves.filter(move => areCoordinatesEqual(move[0], this.state.selectedTile));

		var offsetMoveToAnimate;
		var moveToAnimateJumped;

		// *clears throat*
		if (this.props.moveToAnimate) {
			offsetMoveToAnimate = [
				[(2 * this.props.moveToAnimate[0][0]) + ((this.props.moveToAnimate[0][1] + 1) % 2), this.props.moveToAnimate[0][1]],
				[(2 * this.props.moveToAnimate[1][0]) + ((this.props.moveToAnimate[1][1] + 1) % 2), this.props.moveToAnimate[1][1]]
			];
			if (Math.abs(this.props.moveToAnimate[0][1] - this.props.moveToAnimate[1][1]) === 2) {
				var jumpedX = offsetMoveToAnimate[0][0] > offsetMoveToAnimate[1][0]
					? offsetMoveToAnimate[0][0] - 1
					: offsetMoveToAnimate[0][0] + 1;
				var jumpedY = offsetMoveToAnimate[0][1] > offsetMoveToAnimate[1][1]
					? offsetMoveToAnimate[0][1] - 1
					: offsetMoveToAnimate[0][1] + 1;
				moveToAnimateJumped = [jumpedX, jumpedY];
			}
		}

		var i = 0;
		for (var y = 0; y < 8; y++) {

			var row = [];
			for (var x = 0; x < 8; x++) {

				i++;

				// Check if the tile is one of the playable tiles,
				// if it isn't then we can skip some expensive logic.
				var playable = x % 2 !== y % 2;
				if (playable) {

					// Convert the actual X value into the condensed value the server uses
					var offsetX = (x - 1 + y % 2) / 2;

					var tile = this.props.board.tiles[y][offsetX];
					let coords = [offsetX, y];

					// Check if the valid moves allows this piece to be selectable
					var selectable = !this.props.readonly && this.props.board.validMoves.some(move => areCoordinatesEqual(move[0], coords));
					// Check if this piece can take another and thus must be moved
					var forced = selectable && this.props.board.nextMoveMustBeJump;
					// Check if this piece is the currently selected piece
					var selected = selectable && this.state.selectedTile ? areCoordinatesEqual(this.state.selectedTile, coords) : false;
					// Check if this tile was part of the previous move

					var previous = false;
					if (this.props.moveToAnimate) {

						// If the board's TurnMoves are part of the current turn we want to mark the tiles specified
						previous = this.props.board.turnMoves.length > 1 && this.props.board.turnMoves.some(move => move.some(moveCoords => areCoordinatesEqual(moveCoords, coords)));

						// If the move being animated involves this tile we want to mark it
						if (!previous) previous = this.props.moveToAnimate.some(moveCoords => areCoordinatesEqual(moveCoords, coords));

					} else {

						// If we're not animating a move at the moment just mark the tiles described by the board's TurnMoves
						previous = this.props.board.turnMoves.some(move => move.some(moveCoords => areCoordinatesEqual(moveCoords, coords)));
					}

					// Check if the selected piece could move onto this tile
					var destination = possibleSelectedTileMoves && possibleSelectedTileMoves.some(move => areCoordinatesEqual(move[1], coords));
					var forcedDestination = destination && this.props.board.nextMoveMustBeJump;

					var transformX = 0;
					var transformY = 0;
					if (this.props.moveToAnimate && areCoordinatesEqual(this.props.moveToAnimate[0], coords)) {
						console.log(offsetMoveToAnimate);
						transformX = (offsetMoveToAnimate[1][0] - x) * (this.props.flip ? -1 : 1);
						transformY = (offsetMoveToAnimate[1][1] - y) * (this.props.flip ? -1 : 1);
					}

					var taken = moveToAnimateJumped && areCoordinatesEqual(moveToAnimateJumped, [x, y]);

					// Pass all of the calculated values to a Tile component and add it to the row
					row.push((
						<Tile
							coords={coords}
							tile={tile}
							playable={true}
							selectable={selectable}
							forced={forced}
							selected={selected}
							previous={previous}
							destination={destination}
							forcedDestination={forcedDestination}
							pieceTransformX={transformX}
							pieceTransformY={transformY}
							pieceTaken={taken}
							onClick={(tileCoords) => this.onTileClicked(tileCoords)}
							key={i} />));
				} else {
					row.push((
						<Tile
							playable={false}
							onClick={(tileCoords) => this.onTileClicked(tileCoords)}
							key={i} />));
				}
			}

			rows.push(
				<div className={styles.row} key={y}>
					{this.props.flip ? row.reverse() : row}
				</div>
			);
		}

		return this.props.flip ? rows.reverse() : rows;
	}

	componentDidUpdate(prevProps) {
		// Prevent infinite update loop by only updating the 
		// selected tile if the board has changed
		if (this.props.board === prevProps.board) return;

		// If all of the valid moves originate from the same tile
		// we might as well select it to make for a smoother user experience
		if (!this.props.readonly &&
			this.props.board.validMoves.length > 0 &&
			this.props.board.validMoves.every(move => areCoordinatesEqual(move[0], this.props.board.validMoves[0][0]))) {
			this.setState({ selectedTile: this.props.board.validMoves[0][0] });
		}
	}

	onTileClicked(coords) {

		if (this.props.readonly) {
			return;
		}

		// A non-playable tile won't pass coordinates so unselect the previously selected tile and return
		if (!coords) {
			this.setState({ selectedTile: null });
			return;
		}

		// Check to see if clicking this tile completes a move starting from the previously selected tile
		var completesMove = this.state.selectedTile
			&& this.props.board.validMoves
				.filter(move => areCoordinatesEqual(move[0], this.state.selectedTile))
				.some(move => areCoordinatesEqual(move[1], coords));

		if (completesMove) {
			var tile = this.state.selectedTile;
			this.setState({ selectedTile: null });

			// Tell the parent component (pages/Game) that the player has taken a move
			this.props.onMoveTaken(tile, coords);

		} else {

			// Check to see if this tile can be selected
			var selectable = this.props.board.validMoves.some(move => areCoordinatesEqual(move[0], coords));

			if (selectable) {
				this.setState({ selectedTile: coords });
			} else {
				this.setState({ selectedTile: null });
			}
		}
	}
}

// We can't compare two arrays with === because their references (memory pointers) will be compared, not their values
function areCoordinatesEqual(a, b) {
	return a[0] === b[0] && a[1] === b[1];
}
