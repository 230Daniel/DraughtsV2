import React from "react";
import Piece from "./Piece";

import styles from "./Tile.Module.css";

export default class Tile extends React.Component {
	render() {
		// Render the tile with an onClick event which elevates to the parent component (Board)
		// Render a piece within the tile and pass relevant props down to it
		return (
			<div className={this.getClassNames()} onClick={() => this.props.onClick(this.props.coords)}>
				{this.props.playable &&
					<Piece
						piece={this.props.tile}
						selected={this.props.selected} />
				}
			</div>
		);
	}

	// Returns a className based on this component's props
	// Eg. if "selected=true" is passed then the selected class is added
	getClassNames() {
		if (this.props.playable) {
			var classNames = [styles.tile, styles.white];
			if (this.props.selectable) classNames.push(styles.selectable);
			if (this.props.selected) classNames.push(styles.selected);
			return classNames.join(" ");
		} else {
			return `${styles.tile} ${styles.black}`;
		}
	}
}
