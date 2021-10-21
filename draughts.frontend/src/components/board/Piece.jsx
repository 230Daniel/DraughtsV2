import React from "react";

import styles from "./Piece.Module.css";

export default class Piece extends React.Component{
	render(){
		if (this.props.piece === -1)
			return null;

		return(
			<div className={`
				${styles.piece} 
				${this.props.piece % 2 === 0 ? styles.black : styles.white}
				${this.props.selected ? styles.selected : null}
				`}>
				<div className={styles.inside}>

				</div>
			</div>
		)
	}
}
