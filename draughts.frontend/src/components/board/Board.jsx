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
					var previous = this.props.board.turnMoves.some(move => areCoordinatesEqual(move[0], coords) || areCoordinatesEqual(move[1], coords));
					// Check if the selected piece could move onto this tile
					var destination = possibleSelectedTileMoves && possibleSelectedTileMoves.some(move => areCoordinatesEqual(move[1], coords));
					var forcedDestination = destination && this.props.board.nextMoveMustBeJump;

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
			// Tell the parent component (pages/Game) that the player has taken a move
			this.props.onMoveTaken(this.state.selectedTile, coords);
			this.setState({ selectedTile: null });
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
