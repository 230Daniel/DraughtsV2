import React from "react";

import Tile from "./Tile";
import UnplayableTile from "./UnplayableTile";

import styles from "./Board.Module.css";

export default class Board extends React.Component{
	constructor(props){
		super(props);
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
		
		for(var y = 0; y < 8; y++){
			var row = [];
			for(var x = 0; x < 8; x++){
				if (x % 2 !== y % 2){
					var offsetX = (x - 1 + y % 2) / 2;
					row.push((<Tile key={x} x={offsetX} y={y}/>))
				} else {
					row.push((<UnplayableTile key={x}/>))
				}
				
			}
			rows.push(
				<div className={styles.row}>
					{row}
				</div>
			);
		}

		return rows;
	}
}
