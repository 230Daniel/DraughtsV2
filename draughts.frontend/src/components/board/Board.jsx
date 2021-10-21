import React from "react";

import Tile from "./Tile";

import styles from "./Board.Module.css";

export default class Board extends React.Component{
	constructor(props){
		super(props);
		this.state = {
			selectedTile: null
		}
	}

	render(){
		return(
			<div className={styles.container}>
				<div className={styles.board}>
					{this.renderRows()}
				</div>
			</div>
		)
	}

	renderRows(){
		var rows = [];
		
		for (var y = 0; y < 8; y++) {

			var row = [];
			for (var x = 0; x < 8; x++) {
				
				var playable = x % 2 !== y % 2;
				if (playable) {

					var offsetX = (x - 1 + y % 2) / 2;
					var tile = this.props.board.tiles[y][offsetX];
					var coords = [offsetX, y];

					var selectable = this.props.board.validMoves.some(move => areCoordinatesEqual(move[0], coords));
					var selected = selectable && this.state.selectedTile ? areCoordinatesEqual(this.state.selectedTile, coords) : false;
					
					row.push((
					<Tile 
						coords={coords}
						tile={tile}
						playable={true}
						selectable={selectable}
						selected={selected}
						onClick={(coords) => this.onTileClicked(coords)} 
						key={x}/>))
				} else {
					row.push((
						<Tile
							playable={false}
							onClick={(coords) => this.onTileClicked(coords)} 
							key={x}/>))
				}
			}

			rows.push(
				<div className={styles.row}>
					{this.props.flip ? row.reverse() : row}
				</div>
			);
		}

		return this.props.flip ? rows.reverse() : rows;
	}

	onTileClicked(coords){

		if(!coords){
			this.setState({selectedTile: null});
			return;
		}

		var completesMove = this.state.selectedTile
			&& this.props.board.validMoves
				.filter(move => areCoordinatesEqual(move[0], this.state.selectedTile))
				.some(move => areCoordinatesEqual(move[1], coords));
				
		if (completesMove) {
			console.log(`Made move ${this.state.selectedTile} to ${coords}`)
			this.setState({selectedTile: null});
		} else {
			var selectable = this.props.board.validMoves.some(move => areCoordinatesEqual(move[0], coords));

			if (selectable) {
				this.setState({selectedTile: coords});
			} else {
				this.setState({selectedTile: null});
			}
		}
	}

	
}

function areCoordinatesEqual(a, b){
	return a[0] === b[0] && a[1] === b[1];
}
